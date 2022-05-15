# 91APP 面試後續

感謝 **Andrew** 的提點，

針對交易的抽象話設計一議題之討論，

討論內容如下：

 -  如何制定一抽象化設計，使得交易時得以正確的檢查庫存是否足夠，或是使用者持有此折價券數量是否足夠等。
 -  將折價券視為購物車商品之一部分。

---

以下為實作思路：

 - 定義一介面 `ISellable` 表示所有可參與購買行為之物品。
    - `ISellable` 需決定如何檢查是否可購買(庫存是否足夠、使用者持有數量使否足夠等）。
    - `ISellable` 需決定購買行為執行時如何減少庫存。
    - `ISellable` 需決定當購買行為失效時需如何處置。

 - 定義 `Cart` 作為購物車容器
 - 定義 `Order` 作為購物後結果清單(紀錄)
 - 定義 `API` 作為對外呼叫接口

對於對資料庫之行為，統一以 `IDataBase` 取代直接呼叫 DataBase。

而對於庫存之紀錄，我選擇以增加紀錄的方式記錄，避免直接對於數量進行加減以防止誤算。

---

具體實作內容如下：

首先是最重要的 `API.PlaceOrder`
```C#

    public static Order PlaceOrder(User user, Cart cart, IDataBase db) {
        Order order = new Order();
        order.State = OrderState.Success;

        bool success = true;
        try {
            foreach (var item in cart.Items.OrderBy(item => item.Priority)) {
                if (item.CanBuy(user, cart, db)) {
                    item.Buy(user, cart, db);
                    order.AddItem(item);
                }
                else {
                    success = false;
                    break;
                }
            }
        }
        catch (InvalidOperationException) {
            success = false;
        }
        catch (ArgumentNullException) {
            success = false;
        }

        if (!success) {
            foreach (var item in order.Items) {
                item.Discard(user, cart, db);
            }
            order.State = OrderState.Failure;
        }
        return order;
    }

```

`ISellable` 定義
```C#

ppublic interface ISellable {
    int PId { get; set; }               // 元數據
    string Name { get; set; }           // 元數據
    string Description { get; set; }    // 元數據
    string Tag { get; set; }            // 元數據
    int Price { get; set; }             // 元數據
    int Priority { get; set; }          // 元數據

    void Buy(User user, Cart cart, IDataBase db);      // 行為：購買(減少庫存)
    bool CanBuy(User user, Cart cart, IDataBase db);   // 行為：檢查(檢查庫存)
    void Discard(User user, Cart cart, IDataBase db);  // 行為：取消(庫存回朔)
}

```

商品 `Product` 實作
```C#

public class Product : ISellable {
    public int PId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tag { get; set; }
    public int Price { get; set; }
    public int Priority { get; set; }

    public Product(int pId, IDataBase db) {
        PId = pId;
        (Name, Description, Tag, Price, Priority) = db.GetMetadata(PId);
    }

    public void Buy(User user, Cart cart, IDataBase db) {
        db.Delete(User.Inventory, PId, 1);
        Console.WriteLine($"商品 {Name} x 1 件，購買成功");
    }

    public bool CanBuy(User user, Cart cart, IDataBase db) {
        int remaining = db.SearchCount(User.Inventory, PId);
        Console.WriteLine();
        Console.WriteLine($"商品 {Name} x 1 件，存貨數量： {remaining}");
        if (remaining - 1 < 0) {
            Console.WriteLine($"商品 {Name} 存貨不足");
            return false;
        }
        return true;
    }

    public void Discard(User user, Cart cart, IDataBase db) {
        db.Insert(User.Inventory, PId, 1);
        Console.WriteLine($"商品 {Name} x 1 件，取消購買");
    }
}

```

對於 **存貨** 的處理，我的作法是將其視為一樣是一個 `User` ，一樣對資料庫尋找結果。

再來是折價券(打折) `CouponNPercentOff` 實作
```C#

public class CouponNPercentOff : ISellable {
    public int PId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tag { get; set; }
    public int Price { get; set; }
    public int Priority { get; set; }

    private int percentage;

    public CouponNPercentOff(int pId, IDataBase db) {
        PId = pId;
        (Name, Description, Tag, Price, Priority) = db.GetMetadata(PId);
        if (db.GetCouponInfo(pId) is int p)
            percentage = p;
        else
            throw new ArgumentNullException(nameof(percentage));
    }

    public void Buy(User user, Cart cart, IDataBase db) {
        db.Delete(user, PId, 1);
        var total_price = cart.Items.Select(item => item.Price).Sum();
        Price = -(int)Math.Floor(total_price * (1 - percentage / 100d));
        Console.WriteLine($"折價券 {Name} x 1 張，使用成功");
    }

    public bool CanBuy(User user, Cart cart, IDataBase db) {
        int remaining = db.SearchCount(user, PId);
        Console.WriteLine();
        Console.WriteLine($"折價券 {Name} x 1 張，使用者 (UId: {user.UId}) 持有數量： {remaining}");
        if (remaining - 1 < 0) {
            Console.WriteLine($"折價券 {Name} 使用者持有數量不足");
            return false;
        }
        return true;
    }

    public void Discard(User user, Cart cart, IDataBase db) {
        db.Insert(user, PId, 1);
        Price = 0;
        Console.WriteLine($"折價券 {Name} x 1 張，取消使用");
    }
}

```

折價券就變成是對 `user` 本身做持有數量的檢查，但邏輯一樣，皆是透過 `IDataBase` 去取得所需資料。

資料庫介面 `IDataBase`
```C#

public interface IDataBase {
	void Insert(User user, int pId, int amount);
	void Delete(User user, int pId, int amount);
	int SearchCount(User user, int pId);
	(string, string, string, int, int) GetMetadata(int pId);
	object GetCouponInfo(int pId);
}

```

資料庫部分我用 `Dictionary` 來作為簡單的 `table` ：
```C#

public class SimpleDB : IDataBase {
	public const int TISSUE_ID = 1;
	public const int APPLE_ID = 2;
	public const int MANGO_ID = 3;
	public const int COUPON_75_OFF_ID = 4;

	public const int PRODUCT_PRIORITY = -1;

	public static readonly int[] InventoryProducts = new int[] { TISSUE_ID, APPLE_ID, MANGO_ID, };
	public static readonly int[] UserCoupons = new int[] { COUPON_75_OFF_ID };

	private int pk = 0;
	private Dictionary<int, (User, int, int)> records = new();
	private Dictionary<int, (string, string, string, int, int)> product_table = new() {
		[TISSUE_ID] = ("柔芙輕巧包抽取式衛生紙", "120抽x20包x4袋", "【箱購】", 580, PRODUCT_PRIORITY),
		[APPLE_ID] = ("智利富士蘋果", "135g", "", 14, PRODUCT_PRIORITY),
		[MANGO_ID] = ("頂級愛文芒果禮盒 ", "(約1.6kg/盒)", "【預購】", 468, PRODUCT_PRIORITY),
		[COUPON_75_OFF_ID] = ("消費75折", "不限金額", "【折價券】", 0, 1),
	};
	private Dictionary<int, object> coupon_info = new() {
		[COUPON_75_OFF_ID] = 75,
	};

	public void Insert(User user, int pId, int amount) {
		records.Add(++pk, (user, pId, amount));
	}

	public void Delete(User user, int pId, int amount) {
		var remaining = records.Values.Where(r => r.Item1 == user && r.Item2 == pId).Select(r => r.Item3).Sum();
		if (remaining - amount < 0)
			throw new InvalidOperationException();
		records.Add(++pk, (user, pId, -amount));
	}

	public int SearchCount(User user, int pId) {
		return records.Values.Where(r => r.Item1 == user && r.Item2 == pId).Select(r => r.Item3).Sum();
	}

	public (string, string, string, int, int) GetMetadata(int pId) {
		return product_table[pId];
	}

	public object GetCouponInfo(int pId) {
		return coupon_info[pId];
	}
}

```

如果說單純只是要做 **庫存檢查** & **庫存增減** 的話我想到這一步應該就可以了

但是如果再稍微多考慮一些就會發現上面的設計有可能不太夠

>例如：`滿千折百`，`買一送一`...等，需要有多張折價券交互進行比對的情況

我的考量是：折價券的互相綁定，通常是屬於同一類別的會無法交互使用。

比如 `滿千折百` 滿 1000 元可以使用 1 張，滿 2000 元才可以使用 2 張

而此時如果我又有一張 `母親節特惠，滿一千元享九五折優惠` 

就不用滿 3000 元，只要扣完 `滿千折百` 

剩下總金額仍有超過 1000 元即可享有優惠（假設 `滿千折百` 的優先序較高）

故我選擇用 `IRebateEverySpend` 來作為 `滿X折X` 這類的折價券是否符合使用資格

```C#

internal interface IRebateEverySpend {
    public int EverySpend { get; set; }
    public bool Used { get; set; }
}

```

`CouponRebateEverySpend(滿千折百)` 實作

```C#

public class CouponRebateEverySpend : ISellable, IRebateEverySpend {
    public int PId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tag { get; set; }
    public int Price { get; set; }
    public int Priority { get; set; }

    public int EverySpend { get; set; }
    public bool Used { get; set; }

    private readonly int rebate = 0;

    public CouponRebateEverySpend(int pId, IDataBase db) {
        PId = pId;
        (Name, Description, Tag, Price, Priority) = db.GetMetadata(PId);

        if (db.GetCouponInfo(pId) is ValueTuple<int, int> p)
            (EverySpend, rebate) = p;
        else
            throw new ArgumentNullException(nameof(rebate));
    }

    public void Buy(User user, Cart cart, IDataBase db) {
        db.Delete(user, PId, 1);
        Price = -rebate;
        Used = true;
        Console.WriteLine($"折價券 {Name} x 1 張，使用成功");
    }

    public bool CanBuy(User user, Cart cart, IDataBase db) {
        int remaining = db.SearchCount(user, PId);
        Console.WriteLine();
        Console.WriteLine($"折價券 {Name} x 1 張，使用者 (UId: {user.UId}) 持有數量： {remaining}");
        if (remaining - 1 < 0) {
            Console.WriteLine($"折價券 {Name} 使用者持有數量不足");
            return false;
        }

        int total_spend = 
            cart.Items.Where(item => item is not IRebateEverySpend)
                      .Select(item => item.Price)
                      .Sum();
        int total_rebate_needed =
            cart.Items.Where(item => item is IRebateEverySpend everySpend && everySpend.Used)
                      .Cast<IRebateEverySpend>()
                      .Select(item => item.EverySpend)
                      .Sum();
        int spend_remaining = total_spend - total_rebate_needed;
        if (spend_remaining < EverySpend) {
            Console.WriteLine($"折價券 {Name} 條件不符，尚需 {EverySpend - spend_remaining} 元");
            return false;
        }

        return true;
    }

    public void Discard(User user, Cart cart, IDataBase db) {
        db.Insert(user, PId, 1);
        Price = 0;
        Used = false;
        Console.WriteLine($"折價券 {Name} x 1 張，取消使用");
    }
}

```
---

最後，來看看實際呼叫案例：
```C#

User user = new User(9527);

IDataBase db = new SimpleDB();
db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
db.Insert(User.Inventory, SimpleDB.APPLE_ID, 5);
db.Insert(User.Inventory, SimpleDB.MANGO_ID, 5);
db.Insert(user, SimpleDB.COUPON_75_OFF_ID, 5);
db.Insert(user, SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, 5);

Cart cart = new Cart();
cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
cart.AddItem(new Product(SimpleDB.APPLE_ID, db));
cart.AddItem(new Product(SimpleDB.APPLE_ID, db));
cart.AddItem(new Product(SimpleDB.MANGO_ID, db));
cart.AddItem(new Product(SimpleDB.MANGO_ID, db));
cart.AddItem(new Product(SimpleDB.MANGO_ID, db));
cart.AddItem(new Product(SimpleDB.MANGO_ID, db));
cart.AddItem(new Product(SimpleDB.MANGO_ID, db));
cart.AddItem(new CouponNPercentOff(SimpleDB.COUPON_75_OFF_ID, db));
cart.AddItem(new CouponRebateEverySpend(SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, db));
cart.AddItem(new CouponRebateEverySpend(SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, db));

API.ShowInventory(user, db);

API.ShowCart(user, cart, db);

var order = API.PlaceOrder(user, cart, db);

API.ShowOrder(user, order, db);

API.ShowInventory(user, db);

```

輸出如下：
```
|　　　　　　使用者 |　　　　　　　　　　　名稱 |        數量 |
|            庫存 |　　柔芙輕巧包抽取式衛生紙 |          5 |
|            庫存 |　　　　　　　智利富士蘋果 |          5 |
|            庫存 |　　　　　頂級愛文芒果禮盒 |          5 |
|　 　UId: (9527) |               消費75折 |          5 |
|　　 UId: (9527) | 　　　        折價100元 |          5 |

--------------------------------------

使用者(UId:9527) 購物車內容物如下：

【箱購】 柔芙輕巧包抽取式衛生紙 120抽x20包x4袋 NT$580.00
【箱購】 柔芙輕巧包抽取式衛生紙 120抽x20包x4袋 NT$580.00
【箱購】 柔芙輕巧包抽取式衛生紙 120抽x20包x4袋 NT$580.00
 智利富士蘋果 135g NT$14.00
 智利富士蘋果 135g NT$14.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【折價券】 消費75折 不限金額 NT$0.00
【折價券】 折價100元 消費滿1000元 NT$0.00
【折價券】 折價100元 消費滿1000元 NT$0.00
總金額：4108

======================================


商品 柔芙輕巧包抽取式衛生紙 x 1 件，存貨數量： 5
商品 柔芙輕巧包抽取式衛生紙 x 1 件，購買成功

商品 柔芙輕巧包抽取式衛生紙 x 1 件，存貨數量： 4
商品 柔芙輕巧包抽取式衛生紙 x 1 件，購買成功

商品 柔芙輕巧包抽取式衛生紙 x 1 件，存貨數量： 3
商品 柔芙輕巧包抽取式衛生紙 x 1 件，購買成功

商品 智利富士蘋果 x 1 件，存貨數量： 5
商品 智利富士蘋果 x 1 件，購買成功

商品 智利富士蘋果 x 1 件，存貨數量： 4
商品 智利富士蘋果 x 1 件，購買成功

商品 頂級愛文芒果禮盒  x 1 件，存貨數量： 5
商品 頂級愛文芒果禮盒  x 1 件，購買成功

商品 頂級愛文芒果禮盒  x 1 件，存貨數量： 4
商品 頂級愛文芒果禮盒  x 1 件，購買成功

商品 頂級愛文芒果禮盒  x 1 件，存貨數量： 3
商品 頂級愛文芒果禮盒  x 1 件，購買成功

商品 頂級愛文芒果禮盒  x 1 件，存貨數量： 2
商品 頂級愛文芒果禮盒  x 1 件，購買成功

商品 頂級愛文芒果禮盒  x 1 件，存貨數量： 1
商品 頂級愛文芒果禮盒  x 1 件，購買成功

折價券 折價100元 x 1 張，使用者 (UId: 9527) 持有數量： 5
折價券 折價100元 x 1 張，使用成功

折價券 折價100元 x 1 張，使用者 (UId: 9527) 持有數量： 4
折價券 折價100元 x 1 張，使用成功

折價券 消費75折 x 1 張，使用者 (UId: 9527) 持有數量： 5
折價券 消費75折 x 1 張，使用成功

--------------------------------------

訂單成功！

使用者(UId:9527) 訂單內容物如下：

【箱購】 柔芙輕巧包抽取式衛生紙 120抽x20包x4袋 NT$580.00
【箱購】 柔芙輕巧包抽取式衛生紙 120抽x20包x4袋 NT$580.00
【箱購】 柔芙輕巧包抽取式衛生紙 120抽x20包x4袋 NT$580.00
 智利富士蘋果 135g NT$14.00
 智利富士蘋果 135g NT$14.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【預購】 頂級愛文芒果禮盒  (約1.6kg/盒) NT$468.00
【折價券】 折價100元 消費滿1000元 -NT$100.00
【折價券】 折價100元 消費滿1000元 -NT$100.00
【折價券】 消費75折 不限金額 -NT$977.00
總金額：2931

======================================

|　　　　　　使用者 |　　　　　　　　　　　名稱 |        數量 |
|            庫存 |　　柔芙輕巧包抽取式衛生紙 |          2 |
|            庫存 |　　　　　　　智利富士蘋果 |          3 |
|            庫存 |　　　　　頂級愛文芒果禮盒 |          0 |
|　 　UId: (9527) |               消費75折 |          4 |
|　　 UId: (9527) | 　　　        折價100元 |          3 |
```

由於有 `ISellable.Priority` 來取決優先序，顧加入購物車的先後順序並不影響結果。
另外 `Program.cs` 中也有另外 3 個 TestCase ，有興趣也可以直接執行看看。

---

以上，是我對於交易的抽象化設計的做法

相對於直接參考 **Andrew** 的作法

我選擇以我最熟悉的做法來嘗試這項設計，讓 **Andrew** 可以更直接的知道我目前的程度大概在哪裡

若 **Andrew** 如果有時間的話，還請不吝於提點此項做法有哪裡需要再改進的，或是在未來可能會遇到甚麼樣的問題。

最後，再次感謝 **Andrew** 於面試時提供此疑問並邀請我進行實作

在討論與實作的過程中都是不斷的反思與整理過去經驗，受益良多。

Jack Huang



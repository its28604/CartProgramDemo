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

```C#

public interface ISellable {
	int PId { get; set; } 
	string Name { get; set; }
	string Description { get; set; }
	string Tag { get; set; }
	void Buy(User user, Cart cart, IDataBase db);
	bool CanBuy(User user, Cart cart, IDataBase db);
	void Discard(User user, Cart cart, IDataBase db);
}

```

```C#

public class Product : ISellable {
	public int PId { get; set; } 
	public string Name { get; set; }
	public string Description { get; set; }
	public string Tag { get; set; }

	public Product(int PId, IDataBase db) {
		(Name, Description, Tag) = db.GetMetadata(PId);
	}

	public void Buy(User user, Cart cart, IDataBase db) {
		db.Delete(User.Inventory, PId, 1);
	}

	public bool CanBuy(User user, Cart cart, IDataBase db) {
		int remaining = db.SearchCount(User.Inventory, PId);
		return remaining - 1 >= 0;
	}

	public void Discard(User user, Cart cart, IDataBase db) {
		db.Insert(User.Inventory, PId, 1);
	}
}
```
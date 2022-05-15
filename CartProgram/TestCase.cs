using System.Text;

namespace CartProgram;

public static class TestCase {

	public static void Case1() {
		User user = new User(9527);

		IDataBase db = new SimpleDB();
		db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.APPLE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.MANGO_ID, 5);

		Cart cart = new Cart();
		cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
		cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
		cart.AddItem(new Product(SimpleDB.APPLE_ID, db));
		cart.AddItem(new Product(SimpleDB.MANGO_ID, db));

		ShowInventory(user, db);

		ShowCart(user, cart, db);

		var order = API.PlaceOrder(user, cart, db);

		ShowOrder(user, order, db);

		ShowInventory(user, db);
	}

	public static void Case2() {
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

		ShowInventory(user, db);

		ShowCart(user, cart, db);

		var order = API.PlaceOrder(user, cart, db);

		ShowOrder(user, order, db);

		ShowInventory(user, db);
	}

	public static void Case3()
	{
		User user = new User(9527);

		IDataBase db = new SimpleDB();
		db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.APPLE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.MANGO_ID, 5);
		db.Insert(user, SimpleDB.COUPON_75_OFF_ID, 1);

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
		cart.AddItem(new CouponNPercentOff(SimpleDB.COUPON_75_OFF_ID, db));

		ShowInventory(user, db);

		ShowCart(user, cart, db);

		var order = API.PlaceOrder(user, cart, db);

		ShowOrder(user, order, db);

		ShowInventory(user, db);
	}

	public static void Case4()
	{
		User user = new User(9527);

		IDataBase db = new SimpleDB();
		db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.APPLE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.MANGO_ID, 5);
		db.Insert(user, SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, 5);

		Cart cart = new Cart();
		cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
		cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
		cart.AddItem(new CouponRebateEverySpend(SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, db));
		cart.AddItem(new CouponRebateEverySpend(SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, db));

		ShowInventory(user, db);

		ShowCart(user, cart, db);

		var order = API.PlaceOrder(user, cart, db);

		ShowOrder(user, order, db);

		ShowInventory(user, db);
	}


	private static void ShowCart(User user, Cart cart, IDataBase db)
	{
		Console.WriteLine("\n--------------------------------------\n");
		Console.WriteLine($"�ϥΪ�(UId:{user.UId}) �ʪ������e���p�U�G\n");

		foreach (var item in cart.Items)
		{
			Console.WriteLine($"{item.Tag} {item.Name} {item.Description} {item.Price:C}");
		}

		Console.WriteLine($"�`���B�G{cart.Items.Select(item => item.Price).Sum()}");

		Console.WriteLine("\n======================================\n");
	}

	private static void ShowInventory(User user, IDataBase db)
	{
		Console.WriteLine($"| {CPadLeft("�ϥΪ�", 15)} | {CPadLeft("�W��", 30)} | {CPadLeft("�ƶq", 10)} |");
		foreach (var pId in SimpleDB.InventoryProducts)
		{
			var name = db.GetMetadata(pId).Item1;
			var remaining = db.SearchCount(User.Inventory, pId);
			Console.WriteLine($"| {CPadLeft("�w�s", 15)} | {CPadLeft(name, 30)} | {CPadLeft(remaining.ToString(), 10)} |");
		}
		foreach (var pId in SimpleDB.UserCoupons)
		{
			var name = db.GetMetadata(pId).Item1;
			var remaining = db.SearchCount(user, pId);
			Console.WriteLine($"| {CPadLeft($"UId: ({user.UId})", 15)} | {CPadLeft(name, 30)} | {CPadLeft(remaining.ToString(), 10)} |");
		}
	}

	private static void ShowOrder(User user, Order order, IDataBase db)
	{
		Console.WriteLine("\n--------------------------------------\n");
		if (order.State == OrderState.Success)
		{
			Console.WriteLine($"�q�榨�\�I\n");
			Console.WriteLine($"�ϥΪ�(UId:{user.UId}) �q�椺�e���p�U�G\n");

			foreach (var item in order.Items)
			{
				Console.WriteLine($"{item.Tag} {item.Name} {item.Description} {item.Price:C}");
			}

			Console.WriteLine($"�`���B�G{order.Items.Select(item => item.Price).Sum()}");
		}
		else
		{
			Console.WriteLine($"�q�楢�ѡI\n");
		}
		Console.WriteLine("\n======================================\n");
	}

	private static string CPadLeft(string text, int padleft)
	{
		int chinese_amount = (Encoding.Default.GetByteCount(text) - text.Length) / 2;
		int english_amount = text.Length - chinese_amount;
		int _padleft = padleft - (chinese_amount * 2) - english_amount;
		return String.Empty.PadLeft(_padleft) + text;
	}
}
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

		API.ShowInventory(user, db);

		API.ShowCart(user, cart, db);

		var order = API.PlaceOrder(user, cart, db);

		API.ShowOrder(user, order, db);

		API.ShowInventory(user, db);
	}

	public static void Case2() {
		User user = new User(9527);

		IDataBase db = new SimpleDB();
		db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.APPLE_ID, 5);
		db.Insert(User.Inventory, SimpleDB.MANGO_ID, 5);
		db.Insert(user, SimpleDB.COUPON_75_OFF_ID, 5);
		db.Insert(user, SimpleDB.COUPON_100_ID, 5);

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
		cart.AddItem(new Product(SimpleDB.COUPON_100_ID, db));
		cart.AddItem(new Product(SimpleDB.COUPON_100_ID, db));

		API.ShowCart(user, cart, db);

		var order = API.PlaceOrder(user, cart, db);

		API.ShowOrder(user, order, db);
	}

	public static void Case3() {
		User user = new User(9527);

		IDataBase db = new SimpleDB();

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
		cart.AddItem(new Product(SimpleDB.COUPON_75_OFF_ID, db));
		cart.AddItem(new Product(SimpleDB.COUPON_100_ID, db));
		cart.AddItem(new Product(SimpleDB.COUPON_100_ID, db));

		API.ShowCart(user, cart, db);

		var order = API.PlaceOrder(user, cart, db);

		API.ShowOrder(user, order, db);
	}

}
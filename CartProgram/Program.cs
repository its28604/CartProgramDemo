using CartProgram;


User user = new User(9527);

IDataBase db = new SimpleDB();

Cart cart = new Cart();
cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));

API.ShowCart(user, cart, db);

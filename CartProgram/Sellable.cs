namespace CartProgram;

public interface ISellable {
	int PId { get; set; } 
	string Name { get; set; }
	string Description { get; set; }
	string Tag { get; set; }
	void Buy(User user, Cart cart, IDataBase db);
	bool CanBuy(User user, Cart cart, IDataBase db);
	void Discard(User user, Cart cart, IDataBase db);
}

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
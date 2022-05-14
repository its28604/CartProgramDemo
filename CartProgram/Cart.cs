namespace CartProgram;

public class Cart {
	public List<ISellable> Items { get; private set; } = new();
	public void AddItem(ISellable item) {
		Items.Add(item);
	}
}

public class User {
	public static readonly User Inventory = new(-1);

	public int UId { get; set; }

	public User(int uId) {
		UId = uId;
	}
}

public class Order {
	public OrderState State { get; set; }
	public List<ISellable> Items { get; private set; } = new();
	public void AddItem(ISellable item) {
		Items.Add(item);
	}
}

public enum OrderState {
	Success,
	Failure,
}

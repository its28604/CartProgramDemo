using System;

namespace CartProgram;

public static class API {
	public static void ShowCart(User user, Cart cart, IDataBase db) {
		Console.WriteLine("\n--------------------------------------\n");
		Console.WriteLine($"使用者(UId:{user.UId}) 購物車內容物如下：\n");

		foreach (var item in cart.Items) {
			Console.WriteLine($"{item.Tag} {item.Name} {item.Description}");
		}

		Console.WriteLine("\n======================================\n");
	}
	
	public static Order PlaceOrder(User user, Cart cart, IDataBase db) {
		Order order = new Order();
		order.State = OrderState.Success;

		try {
			foreach (var item in cart.Items) {
				if (item.CanBuy(user, cart, db)) {
					item.Buy(user, cart, db);
					order.AddItem(item);
				}
				else {
					throw new InvalidOperationException();
				}
			}
		} catch (InvalidOperationException) {
			foreach (var item in order.Items) {
				item.Discard(user, cart, db);
			}
			order.State = OrderState.Failure;
		}
		return order;
	}
}
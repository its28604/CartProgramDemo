using System.Text;

namespace CartProgram;

public static class API
{
    public static void ShowCart(User user, Cart cart, IDataBase db)
    {
        Console.WriteLine("\n--------------------------------------\n");
        Console.WriteLine($"使用者(UId:{user.UId}) 購物車內容物如下：\n");

        foreach (var item in cart.Items)
        {
            Console.WriteLine($"{item.Tag} {item.Name} {item.Description} {item.Price:C}");
        }
        
        Console.WriteLine($"總金額：{cart.Items.Select(item => item.Price).Sum()}");

        Console.WriteLine("\n======================================\n");
    }

    public static void ShowInventory(User user, IDataBase db)
    {
        Console.WriteLine($"| {CPadLeft("使用者", 15)} | {CPadLeft("名稱", 30)} | {CPadLeft("數量", 10)} |");
        foreach (var pId in SimpleDB.InventoryProducts)
        {
            var name = db.GetMetadata(pId).Item1;
            var remaining = db.SearchCount(User.Inventory, pId);
            Console.WriteLine($"| {CPadLeft("庫存", 15)} | {CPadLeft(name, 30)} | {CPadLeft(remaining.ToString(), 10)} |");
        }
        foreach (var pId in SimpleDB.UserCoupons)
        {
            var name = db.GetMetadata(pId).Item1;
            var remaining = db.SearchCount(user, pId);
            Console.WriteLine($"| {CPadLeft($"UId: ({user.UId})", 15)} | {CPadLeft(name, 30)} | {CPadLeft(remaining.ToString(), 10)} |");
        }
    }

    public static void ShowOrder(User user, Order order, IDataBase db)
    {
        Console.WriteLine("\n--------------------------------------\n");
        if (order.State == OrderState.Success)
        {
            Console.WriteLine($"訂單成功！\n");
            Console.WriteLine($"使用者(UId:{user.UId}) 訂單內容物如下：\n");

            foreach (var item in order.Items)
            {
                Console.WriteLine($"{item.Tag} {item.Name} {item.Description} {item.Price:C}");
            }

            Console.WriteLine($"總金額：{order.Items.Select(item => item.Price).Sum()}");
        }
        else
        {
            Console.WriteLine($"訂單失敗！\n");
        }
        Console.WriteLine("\n======================================\n");
    }

    public static Order PlaceOrder(User user, Cart cart, IDataBase db)
    {
        Order order = new Order();
        order.State = OrderState.Success;

        bool success = true;
        try
        {
            foreach (var item in cart.Items.OrderBy(item => item.Priority))
            {
                if (item.CanBuy(user, cart, db))
                {
                    item.Buy(user, cart, db);
                    order.AddItem(item);
                }
                else
                {
                    success = false;
                    break;
                }
            }
        }
        catch (InvalidOperationException)
        {
            success = false;
        }
        catch (ArgumentNullException)
        {
            success = false;
        }

        if (!success)
        {
            foreach (var item in order.Items)
            {
                item.Discard(user, cart, db);
            }
            order.State = OrderState.Failure;
        }
        return order;
    }

    private static string CPadLeft(string text, int padleft)
    {
        int chinese_amount = (Encoding.Default.GetByteCount(text) - text.Length) / 2;
        int english_amount = text.Length - chinese_amount;
        int _padleft = padleft - (chinese_amount * 2) - english_amount;
        return String.Empty.PadLeft(_padleft) + text;
    }
}
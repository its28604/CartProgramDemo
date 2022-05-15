namespace CartProgram;

public static class API
{
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
}
namespace CartProgram;

public interface ISellable
{
    int PId { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    string Tag { get; set; }
    int Price { get; set; }
    int Priority { get; set; }

    void Buy(User user, Cart cart, IDataBase db);
    bool CanBuy(User user, Cart cart, IDataBase db);
    void Discard(User user, Cart cart, IDataBase db);
}

public class Product : ISellable
{
    public int PId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tag { get; set; }
    public int Price { get; set; }
    public int Priority { get; set; }

    public Product(int pId, IDataBase db)
    {
        PId = pId;
        (Name, Description, Tag, Price, Priority) = db.GetMetadata(PId);
    }

    public void Buy(User user, Cart cart, IDataBase db)
    {
        db.Delete(User.Inventory, PId, 1);
        Console.WriteLine($"商品 {Name} x 1 件，購買成功");
    }

    public bool CanBuy(User user, Cart cart, IDataBase db)
    {
        int remaining = db.SearchCount(User.Inventory, PId);
        Console.WriteLine();
        Console.WriteLine($"商品 {Name} x 1 件，存貨數量： {remaining}");
        if (remaining - 1 < 0)
        {
            Console.WriteLine($"商品 {Name} 存貨不足");
            return false;
        }
        return true;
    }

    public void Discard(User user, Cart cart, IDataBase db)
    {
        db.Insert(User.Inventory, PId, 1);
        Console.WriteLine($"商品 {Name} x 1 件，取消購買");
    }
}

public class CouponNPercentOff : ISellable
{
    public int PId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tag { get; set; }
    public int Price { get; set; }
    public int Priority { get; set; }

    private readonly int percentage;

    public CouponNPercentOff(int pId, IDataBase db)
    {
        PId = pId;
        (Name, Description, Tag, Price, Priority) = db.GetMetadata(PId);
        if (db.GetCouponInfo(pId) is int p)
            percentage = p;
        else
            throw new ArgumentNullException(nameof(percentage));
    }

    public void Buy(User user, Cart cart, IDataBase db)
    {
        db.Delete(user, PId, 1);
        var total_price = cart.Items.Select(item => item.Price).Sum();
        Price = -(int)Math.Floor(total_price * (1 - percentage / 100d));
        Console.WriteLine($"折價券 {Name} x 1 張，使用成功");
    }

    public bool CanBuy(User user, Cart cart, IDataBase db)
    {
        int remaining = db.SearchCount(user, PId);
        Console.WriteLine();
        Console.WriteLine($"折價券 {Name} x 1 張，使用者 (UId: {user.UId}) 持有數量： {remaining}");
        if (remaining - 1 < 0)
        {
            Console.WriteLine($"折價券 {Name} 使用者持有數量不足");
            return false;
        }
        return true;
    }

    public void Discard(User user, Cart cart, IDataBase db)
    {
        db.Insert(user, PId, 1);
        Price = 0;
        Console.WriteLine($"折價券 {Name} x 1 張，取消使用");
    }
}

internal interface IRebateEverySpend
{
    public int EverySpend { get; set; }
    public bool Used { get; set; }
}

public class CouponRebateEverySpend : ISellable, IRebateEverySpend
{
    public int PId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tag { get; set; }
    public int Price { get; set; }
    public int Priority { get; set; }

    public int EverySpend { get; set; }
    public bool Used { get; set; }

    private readonly int rebate = 0;

    public CouponRebateEverySpend(int pId, IDataBase db)
    {
        PId = pId;
        (Name, Description, Tag, Price, Priority) = db.GetMetadata(PId);

        if (db.GetCouponInfo(pId) is ValueTuple<int, int> p)
            (EverySpend, rebate) = p;
        else
            throw new ArgumentNullException(nameof(rebate));
    }

    public void Buy(User user, Cart cart, IDataBase db)
    {
        db.Delete(user, PId, 1);
        Price = -rebate;
        Used = true;
        Console.WriteLine($"折價券 {Name} x 1 張，使用成功");
    }

    public bool CanBuy(User user, Cart cart, IDataBase db)
    {
        int remaining = db.SearchCount(user, PId);
        Console.WriteLine();
        Console.WriteLine($"折價券 {Name} x 1 張，使用者 (UId: {user.UId}) 持有數量： {remaining}");
        if (remaining - 1 < 0)
        {
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
        if (spend_remaining < EverySpend)
        {
            Console.WriteLine($"折價券 {Name} 條件不符，尚需 {EverySpend - spend_remaining} 元");
            return false;
        }

        return true;
    }

    public void Discard(User user, Cart cart, IDataBase db)
    {
        db.Insert(user, PId, 1);
        Price = 0;
        Used = false;
        Console.WriteLine($"折價券 {Name} x 1 張，取消使用");
    }
}
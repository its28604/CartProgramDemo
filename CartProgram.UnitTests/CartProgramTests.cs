using CartProgram;
using Xunit;

namespace CartProgram.UnitTests;

public class CartProgramTests
{
    private readonly User USER_A = new User(9527);

    [Fact]
    public void IDataBaseTest()
    {
        // Arrange
        IDataBase db = new SimpleDB();
        db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);

        // Assert
        Assert.Equal(5, db.SearchCount(User.Inventory, SimpleDB.TISSUE_ID));
    }

    [Fact]
    public void PlaceOrder()
    {
        // Arrange
        IDataBase db = new SimpleDB();
        db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);

        Cart cart = new Cart();
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));

        // Act
        var order = API.PlaceOrder(USER_A, cart, db);

        Assert.Equal(3, db.SearchCount(User.Inventory, SimpleDB.TISSUE_ID));
        Assert.Equal(OrderState.Success, order.State);
    }

    [Fact]
    public void PlaceOrder_WithCouponPercentOff()
    {
        // Arrange
        IDataBase db = new SimpleDB();
        db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
        db.Insert(USER_A, SimpleDB.COUPON_75_OFF_ID, 1);

        Cart cart = new Cart();
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new CouponNPercentOff(SimpleDB.COUPON_75_OFF_ID, db));

        // Act
        var order = API.PlaceOrder(USER_A, cart, db);

        Assert.Equal(3, db.SearchCount(User.Inventory, SimpleDB.TISSUE_ID));
        Assert.Equal(OrderState.Success, order.State);
    }

    [Fact]
    public void PlaceOrder_WithCouponDiscount()
    {
        // Arrange
        IDataBase db = new SimpleDB();
        db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
        db.Insert(USER_A, SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, 1);

        Cart cart = new Cart();
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new CouponRebateEverySpend(SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, db));

        // Act
        var order = API.PlaceOrder(USER_A, cart, db);

        Assert.Equal(3, db.SearchCount(User.Inventory, SimpleDB.TISSUE_ID));
        Assert.Equal(OrderState.Success, order.State);
    }

    [Fact]
    public void PlaceOrder_WithCouponDiscount_WhenNotEnough()
    {
        // Arrange
        IDataBase db = new SimpleDB();
        db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);
        db.Insert(USER_A, SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, 2);

        Cart cart = new Cart();
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new CouponRebateEverySpend(SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, db));
        cart.AddItem(new CouponRebateEverySpend(SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID, db));

        // Act
        var order = API.PlaceOrder(USER_A, cart, db);

        Assert.Equal(5, db.SearchCount(User.Inventory, SimpleDB.TISSUE_ID));
        Assert.Equal(2, db.SearchCount(USER_A, SimpleDB.COUPON_REBATE_100_FOREVERY_1000_SPEND_ID));
        Assert.Equal(OrderState.Failure, order.State);
    }
}
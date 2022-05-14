using CartProgram;

namespace CartProgramTests;

public class UnitTest1
{
    private readonly User User_A = new User(9527);

    [Fact]
    public void IDataBaseTest() {
        // Arrange
        IDataBase db = new SimpleDB();
        db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);

        // Assert
        Assert.Equal(5, db.SearchCount(User.Inventory, SimpleDB.TISSUE_ID));
    }

    [Fact]
    public void PlaceOrder() {
        // Arrange
        IDataBase db = new SimpleDB();
        db.Insert(User.Inventory, SimpleDB.TISSUE_ID, 5);

        Cart cart = new Cart();
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));
        cart.AddItem(new Product(SimpleDB.TISSUE_ID, db));

        // Act
        var order = API.PlaceOrder(User_A, cart, db);

        Assert.Equal(3, db.SearchCount(User.Inventory, SimpleDB.TISSUE_ID));
        Assert.Equal(OrderState.Success, order.State);
    }
}
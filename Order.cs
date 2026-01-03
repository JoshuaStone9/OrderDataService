internal class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
}

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
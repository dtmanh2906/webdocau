namespace VuaDoCau.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public string ReceiverName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Address { get; set; } = default!;
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public string Status { get; set; } = "Pending";

    public List<OrderItem> Items { get; set; } = new();
    public decimal Total => Subtotal + ShippingFee;
}

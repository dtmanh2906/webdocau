namespace VuaDoCau.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string ImageUrl { get; set; } = "/images/noimg.png";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}
namespace rqrmenu.Areas.Dashboard.Models;

public class CartItem
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public decimal Total => Price * Quantity;
}
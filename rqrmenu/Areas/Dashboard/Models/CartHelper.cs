using Newtonsoft.Json;

namespace rqrmenu.Areas.Dashboard.Models;

public static class CartHelper
{
    public static List<CartItem> GetCartItems(ISession session)
    {
        var cartJson = session.GetString("Cart");
        return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
    }

    public static void SaveCart(ISession session, List<CartItem> cartItems)
    {
        var cartJson = JsonConvert.SerializeObject(cartItems);
        session.SetString("Cart", cartJson);
    }

    public static void AddToCart(ISession session, CartItem item)
    {
        var cartItems = GetCartItems(session);
        var existingItem = cartItems.FirstOrDefault(i => i.ItemId == item.ItemId);

        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            cartItems.Add(item);
        }

        SaveCart(session, cartItems);
    }

    public static void RemoveFromCart(ISession session, int itemId)
    {
        var cartItems = GetCartItems(session);
        cartItems = cartItems.Where(i => i.ItemId != itemId).ToList();
        SaveCart(session, cartItems);
    }

    public static void UpdateQuantity(ISession session, int itemId, int quantity)
    {
        var cartItems = GetCartItems(session);
        var existingItem = cartItems.FirstOrDefault(i => i.ItemId == itemId);

        if (existingItem != null && quantity > 0)
        {
            existingItem.Quantity = quantity;
        }
        else
        {
            cartItems.Remove(existingItem);
        }

        SaveCart(session, cartItems);
    }
}

using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public interface ICartService
{
    List<CartItem> GetCartItems();
    void AddToCart(int productId, int quantity = 1);
    void UpdateQuantity(int productId, int quantity);
    void RemoveFromCart(int productId);
    void ClearCart();
    decimal GetTotal();
    int GetItemCount();
}

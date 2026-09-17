using System.Text.Json;
using HealthcareShoppingCart.Models;

namespace HealthcareShoppingCart.Services;

public class CartService : ICartService
{
    private const string CartSessionKey = "HealthcareCart";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProductService _productService;

    public CartService(IHttpContextAccessor httpContextAccessor, IProductService productService)
    {
        _httpContextAccessor = httpContextAccessor;
        _productService = productService;
    }

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Session is not available.");

    public List<CartItem> GetCartItems()
    {
        var json = Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    private void SaveCart(List<CartItem> items)
    {
        Session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
    }

    public void AddToCart(int productId, int quantity = 1)
    {
        var product = _productService.GetProductById(productId);
        if (product is null || quantity < 1)
        {
            return;
        }

        var cart = GetCartItems();
        var existing = cart.FirstOrDefault(i => i.ProductId == productId);

        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                Quantity = quantity
            });
        }

        SaveCart(cart);
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var cart = GetCartItems();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            cart.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        SaveCart(cart);
    }

    public void RemoveFromCart(int productId)
    {
        var cart = GetCartItems();
        cart.RemoveAll(i => i.ProductId == productId);
        SaveCart(cart);
    }

    public void ClearCart() => Session.Remove(CartSessionKey);

    public decimal GetTotal() => GetCartItems().Sum(i => i.Subtotal);

    public int GetItemCount() => GetCartItems().Sum(i => i.Quantity);
}

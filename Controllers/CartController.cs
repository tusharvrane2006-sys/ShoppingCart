using HealthcareShoppingCart.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareShoppingCart.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        ViewBag.CartItemCount = _cartService.GetItemCount();
        ViewBag.Total = _cartService.GetTotal();
        return View(_cartService.GetCartItems());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        _cartService.UpdateQuantity(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cartService.RemoveFromCart(productId);
        TempData["SuccessMessage"] = "Item removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cartService.ClearCart();
        TempData["SuccessMessage"] = "Cart cleared.";
        return RedirectToAction(nameof(Index));
    }
}

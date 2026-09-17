using HealthcareShoppingCart.Models;
using HealthcareShoppingCart.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareShoppingCart.Controllers;

public class PaymentController : Controller
{
    private readonly ICartService _cartService;

    public PaymentController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        var cartItems = _cartService.GetCartItems();
        if (cartItems.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty. Add products before checkout.";
            return RedirectToAction("Index", "Products");
        }

        ViewBag.CartItemCount = _cartService.GetItemCount();

        var model = new PaymentViewModel
        {
            CartItems = cartItems,
            TotalAmount = _cartService.GetTotal()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Process(PaymentViewModel model)
    {
        var cartItems = _cartService.GetCartItems();
        if (cartItems.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Products");
        }

        model.CartItems = cartItems;
        model.TotalAmount = _cartService.GetTotal();

        if (!ModelState.IsValid)
        {
            ViewBag.CartItemCount = _cartService.GetItemCount();
            return View("Index", model);
        }

        var orderId = $"HC-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
        _cartService.ClearCart();

        TempData["OrderId"] = orderId;
        TempData["PaidAmount"] = model.TotalAmount.ToString("F2");
        TempData["CustomerEmail"] = model.Email;

        return RedirectToAction(nameof(Confirmation));
    }

    public IActionResult Confirmation()
    {
        if (TempData["OrderId"] is null)
        {
            return RedirectToAction("Index", "Products");
        }

        return View();
    }
}

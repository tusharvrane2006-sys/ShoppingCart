using HealthcareShoppingCart.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareShoppingCart.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;

    public ProductsController(IProductService productService, ICartService cartService)
    {
        _productService = productService;
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        ViewBag.CartItemCount = _cartService.GetItemCount();
        return View(_productService.GetAllProducts());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int productId, int quantity = 1)
    {
        _cartService.AddToCart(productId, quantity);
        TempData["SuccessMessage"] = "Product added to cart successfully!";
        return RedirectToAction(nameof(Index));
    }
}

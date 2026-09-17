using HealthcareShoppingCart.Models;
using HealthcareShoppingCart.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareShoppingCart.Controllers;

public class ChatbotController : Controller
{
    private readonly IChatService _chatService;
    private readonly ICartService _cartService;

    public ChatbotController(IChatService chatService, ICartService cartService)
    {
        _chatService = chatService;
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        ViewBag.CartItemCount = _cartService.GetItemCount();
        return View();
    }

    [HttpGet]
    public IActionResult Providers()
    {
        return Json(_chatService.GetAvailableProviders());
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ChatRequest request)
    {
        var response = await _chatService.SendMessageAsync(
            request.Provider,
            request.Message,
            request.History);

        return Json(response);
    }
}

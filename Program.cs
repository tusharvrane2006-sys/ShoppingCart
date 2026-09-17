using HealthcareShoppingCart.Models;
using HealthcareShoppingCart.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<GoogleApiOptions>(
    builder.Configuration.GetSection(GoogleApiOptions.SectionName));
builder.Services.Configure<CursorApiOptions>(
    builder.Configuration.GetSection(CursorApiOptions.SectionName));
builder.Services.Configure<OllamaApiOptions>(
    builder.Configuration.GetSection(OllamaApiOptions.SectionName));
builder.Services.Configure<ChatOptions>(
    builder.Configuration.GetSection(ChatOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<GoogleChatProvider>();
builder.Services.AddHttpClient<CursorChatProvider>();
builder.Services.AddHttpClient<OllamaChatProvider>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IChatProvider>(sp => sp.GetRequiredService<GoogleChatProvider>());
builder.Services.AddScoped<IChatProvider>(sp => sp.GetRequiredService<CursorChatProvider>());
builder.Services.AddScoped<IChatProvider>(sp => sp.GetRequiredService<OllamaChatProvider>());
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

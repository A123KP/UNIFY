using Microsoft.EntityFrameworkCore;
using UNIFY.Data;
using UNIFY.Services;
using Serilog; //added to create a log file
Log.Information("Application Starting...");
Log.Logger = new LoggerConfiguration()
   .WriteTo.Console() // This shows logs in VS output window live
   .WriteTo.File(
       "Logs/unify-log.txt",
       rollingInterval: RollingInterval.Day,
       buffered: false,           // disable buffering for immediate write
       flushToDiskInterval: TimeSpan.FromSeconds(1)) // flush frequently
   .CreateLogger();//usinf rollingInterval to create a new log file every day

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(); // Use Serilog for logging

// Add services to the container.
builder.Services.AddScoped<IUserService, UserService>(); // Existing
builder.Services.AddScoped<IProductService, ProductService>(); // Add this
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IOrderService, OrderService>(); 
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAdminCustomerService, AdminCustomerService>();

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Optional: adjust session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});//required for session
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); //session before route

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

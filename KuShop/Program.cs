using Microsoft.EntityFrameworkCore;
using KuShop.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Session Part
//ÃÐºØ Timeout 2 ªÑèÇâÁ§
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>options.IdleTimeout=TimeSpan.FromHours(2));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//DBContext
builder.Services.AddDbContext<KuShopContext>( options=>options.UseSqlServer(
    builder.Configuration.GetConnectionString("DBConn")
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

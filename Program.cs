using Microsoft.EntityFrameworkCore;
using OrganikMarketProje.Data;
using OrganikMarketProje.Services;
using OrganikMarketProje.Models;
using Microsoft.AspNetCore.Identity;
using OrganikMarketProje.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant�s�
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity ayarlar� ve �zelle�tirilmi� hata mesajlar�
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // �ifre gereksinimleri
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true; // �zel karakter
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Kullan�c� ad� gereksinimleri
    options.User.RequireUniqueEmail = true;

    // Kilitleme ayarlar� (iste�e ba�l�)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<CustomIdentityErrorDescriber>();

// Servisler
builder.Services.AddScoped<ProductOperations>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<CartService>();
builder.Services.AddSession();

// MVC + DataAnnotationsLocalization
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rolleri ve admin kullan�c�y� olu�tur
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    string adminEmail = "admin@admin.com";
    string adminPassword = "Admin123.";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Name = "Admin",
            Surname = "User"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();

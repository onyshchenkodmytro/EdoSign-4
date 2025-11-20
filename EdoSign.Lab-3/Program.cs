using EdoSign.Lab_3.Data;
using EdoSign.Lab_3.Models;
using EdoSign.Signing;
using EdoSign.Lab_3.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// =======================================================
// 1. Database provider switch (SqlServer, Postgres, Sqlite, InMemory)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var config = builder.Configuration;
    var provider = config["DatabaseOptions:Provider"];

    switch (provider)
    {
        case "SqlServer":
            options.UseSqlServer(config.GetConnectionString("SqlServer"));
            break;

        case "Postgres":
            options.UseNpgsql(config.GetConnectionString("Postgres"));
            break;

        case "Sqlite":
            options.UseSqlite(config.GetConnectionString("Sqlite"));
            break;

        case "InMemory":
        default:
            options.UseInMemoryDatabase("EdoSignInMemory");
            break;
    }
});

// =======================================================
// 2. ASP.NET Identity (used for roles, UserManager, etc)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 3; // password now irrelevant
        opt.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// =======================================================
// 3. REMOVE OpenID Connect completely (fake login used instead)
// builder.Services.AddAuthentication...
// (Залишено cookie authentication, бо воно потрібно MVC?)

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// =======================================================
// 4. MVC
builder.Services.AddControllersWithViews();

// =======================================================
// 5. Authorization
builder.Services.AddAuthorization();

// =======================================================
// 6. DI
builder.Services.AddSingleton<ISigner, RsaSigner>();
builder.Services.AddScoped<CryptoService>();

// =======================================================
// 7. Build app
var app = builder.Build();

// =======================================================
// 8. Auto-migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("⚠️ Migration failed: " + ex.Message);
    }
}

// =======================================================
// 🔥 9. FAKE AUTH MIDDLEWARE (AUTOMATIC LOGIN)
// =======================================================
app.Use(async (context, next) =>
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "TestUser"),
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Email, "test@example.com"),
        new Claim("role", "User")
    };

    var identity = new ClaimsIdentity(claims, "FakeAuth");
    context.User = new ClaimsPrincipal(identity);

    await next();
});

// =======================================================
// 10. Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

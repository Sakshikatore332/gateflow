using gateflow.Data;
using gateflow.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GateFlowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "GateFlowConnection")));

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    string? setupPassword =
        Environment.GetEnvironmentVariable(
            "GATEFLOW_ADMIN_SETUP_PASSWORD");

    if (!string.IsNullOrWhiteSpace(setupPassword))
    {
        await GateFlow.Setup.AdminSeed.CreateAdmin(
            app.Services, setupPassword);
    }
}

if (app.Environment.IsDevelopment())
{
    await GateFlow.Setup.AdminSeed.CreateAdmin(
        app.Services, "GateFlow@2026");
}



// TEMPORARY ADMIN PASSWORD RESET
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider
            .GetRequiredService<GateFlowDbContext>();

        var admin = db.Users.FirstOrDefault(
            u => u.Email == "admin@example.com"
                 && u.Role == "Admin");

        if (admin != null)
        {
            var hasher = new PasswordHasher<User>();

            admin.PasswordHash = hasher.HashPassword(
                admin, "Admin@123");

            db.SaveChanges();

            Console.WriteLine("Admin password reset successfully.");
        }
        else
        {
            Console.WriteLine("Admin account not found.");
        }
    }
}

app.Run();

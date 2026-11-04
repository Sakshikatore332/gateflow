using gateflow.Data;
using gateflow.Models;
using GateFlow.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GateFlow.Controllers
{
    public class AccountController : Controller
    {
        private readonly GateFlowDbContext db;

        public AccountController(GateFlowDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLower();

            if (db.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("Email",
                    "Email already registered");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = email,
                Department = model.Department,
                Phone = model.Phone,
                Role = "Student"
            };

            var hasher = new PasswordHasher<User>();

            user.PasswordHash = hasher.HashPassword(
                user, model.Password);

            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] =
                "Registration successful. Please login.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLower();

            var user = db.Users.FirstOrDefault(
                u => u.Email == email && u.Role == "Student");

            if (user == null)
            {
                ModelState.AddModelError("",
                    "Invalid email or password");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();

            PasswordVerificationResult result;

            try
            {
                result = hasher.VerifyHashedPassword(
                    user, user.PasswordHash, model.Password);
            }
            catch (FormatException)
            {
                result = PasswordVerificationResult.Failed;
            }

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("",
                    "Invalid email or password");
                return View(model);
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = hasher.HashPassword(
                    user, model.Password);
                db.SaveChanges();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(ClaimTypes.Name,
                    user.FullName),

                new Claim(ClaimTypes.Role,
                    user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Dashboard", "Student");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content("Access denied. You do not have permission.");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AdminLogin()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLower();

            var admin = await db.Users.FirstOrDefaultAsync(
                u => u.Email == email && u.Role == "Admin");

            if (admin == null)
            {
                ModelState.AddModelError("",
                    "Invalid admin email or password");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();

            PasswordVerificationResult result;

            try
            {
                result = hasher.VerifyHashedPassword(
                    admin, admin.PasswordHash, model.Password);
            }
            catch (FormatException)
            {
                result = PasswordVerificationResult.Failed;
            }

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("",
                    "Invalid admin email or password");
                return View(model);
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                admin.PasswordHash =
                    hasher.HashPassword(admin, model.Password);

                await db.SaveChangesAsync();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier,
            admin.UserId.ToString()),

        new Claim(ClaimTypes.Name, admin.FullName),

        new Claim(ClaimTypes.Role, "Admin")
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Dashboard", "Admin");
        }

    }
}

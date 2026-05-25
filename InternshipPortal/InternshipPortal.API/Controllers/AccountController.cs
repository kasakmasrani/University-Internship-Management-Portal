using System.Security.Claims;
using InternshipPortal.API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers;

public class AccountController : Controller
{
    private static readonly Dictionary<string, (string Password, string Role)> DemoUsers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = ("admin123", "Admin"),
        ["company"] = ("company123", "Company"),
        ["faculty"] = ("faculty123", "Faculty"),
        ["student"] = ("student123", "Student"),
        ["student01"] = ("student123", "Student"),
        ["student02"] = ("student123", "Student"),
        ["student03"] = ("student123", "Student"),
        ["student04"] = ("student123", "Student"),
        ["student05"] = ("student123", "Student"),
        ["student06"] = ("student123", "Student"),
        ["student07"] = ("student123", "Student"),
        ["student08"] = ("student123", "Student"),
        ["student09"] = ("student123", "Student"),
        ["student10"] = ("student123", "Student"),
        ["student11"] = ("student123", "Student"),
        ["student12"] = ("student123", "Student")
    };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!DemoUsers.TryGetValue(model.Username, out var user) || !string.Equals(user.Password, model.Password, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

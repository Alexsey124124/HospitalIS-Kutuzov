using System.Security.Claims;
using HospitalIS.Application.DTOs.Auth;
using HospitalIS.Application.Services.Auth;
using HospitalIS.Application.Services.Specializations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalIS.WebApi.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAuthService _auth;
    private readonly ISpecializationService _specs;

    public AccountController(IAuthService auth, ISpecializationService specs)
    {
        _auth = auth;
        _specs = specs;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(dto);

        var user = await _auth.LoginAsync(dto.Login, dto.Password);
        if (user == null)
        {
            ModelState.AddModelError("", "Неверный логин или пароль");
            return View(dto);
        }

        await SignIn(user);
        TempData["Success"] = $"Добро пожаловать, {user.FullName}!";
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        ViewBag.Specializations = await _specs.GetAllAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Specializations = await _specs.GetAllAsync();
            return View(dto);
        }
        try
        {
            var user = await _auth.RegisterAsync(dto);
            await SignIn(user);
            TempData["Success"] = "Регистрация выполнена. Добро пожаловать!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Specializations = await _specs.GetAllAsync();
            return View(dto);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Вы вышли из системы";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task SignIn(AuthUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role),
            new("FullName", user.FullName)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }
}
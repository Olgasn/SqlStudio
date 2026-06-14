using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlqStudio.Application.Services.JwtServices;
using SlqStudio.Models;

namespace SlqStudio.Controllers;

public class HomeController : BaseMvcController
{
    private readonly IJwtTokenHandler _jwtTokenHandler;

    public HomeController(ILogger<HomeController> logger, IJwtTokenHandler jwtTokenHandler)
        : base(logger)
    {
        _jwtTokenHandler = jwtTokenHandler;
    }

    public IActionResult Index()
    {
        LogInfo("Вход в Home/Index");

        var token = Request.Cookies["jwt"];
        if (string.IsNullOrEmpty(token))
        {
            LogWarning("JWT токен отсутствует — перенаправление на Auth/Login");
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var (email, role, name) = _jwtTokenHandler.GetClaimsFromToken(token);
            LogInfo("JWT токен успешно прочитан", new { email, role, name });

            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserRole", role.ToString());
            HttpContext.Session.SetString("UserName", name);

            return View();
        }
        catch (Exception ex)
        {
            LogError("Ошибка при разборе JWT токена", ex);
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login", "Auth");
        }
    }

    public IActionResult Privacy()
    {
        LogInfo("Открыта страница конфиденциальности");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var errorDescription = "Произошла непредвиденная ошибка. Пожалуйста, попробуйте снова позже.";

        return View(new ErrorViewModel { RequestId = requestId, ErrorDescription = errorDescription });
    }


    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        LogWarning("Доступ запрещён — переход на AccessDenied");
        return View("~/Views/Shared/AccessDenied.cshtml");
    }
}

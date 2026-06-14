using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SlqStudio.Application.CQRS.Course.Commands;
using SlqStudio.Application.CQRS.Course.Queries;
using SlqStudio.Application.Services.AppSettingsServices;

namespace SlqStudio.Controllers;

[AllowAnonymous]
public class ConfigController : BaseMvcController
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly AppSettingsBuilder _appSettingsBuilder;
    private readonly IMediator _mediator;
    private readonly string _appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

    public ConfigController(
        IAppSettingsService appSettingsService,
        AppSettingsBuilder appSettingsBuilder,
        IMediator mediator,
        ILogger<ConfigController> logger
    ) : base(logger)
    {
        _appSettingsService = appSettingsService;
        _appSettingsBuilder = appSettingsBuilder;
        _mediator = mediator;
    }

    public IActionResult Login()
    {
        LogInfo("Открыта страница входа.");
        return View();
    }

    [HttpPost]
    public IActionResult Login(string name, string password)
    {
        LogInfo("Попытка входа", new { name });

        try
        {
            var config = _appSettingsService.ReadConfig(_appSettingsPath);
            var userSection = config["ConfigUser"];

            if (userSection != null &&
                name == userSection["Name"]?.ToString() &&
                password == userSection["Password"]?.ToString())
            {
                HttpContext.Session.SetString("IsConfigAuthorized", "true");
                LogInfo("Успешный вход", new { name });
                return RedirectToAction("EditConfig");
            }

            LogWarning("Неуспешный вход: неверные данные", new { name });
            ViewBag.Error = "Неверное имя пользователя или пароль.";
            return View();
        }
        catch (Exception ex)
        {
            LogError("Ошибка при попытке входа", ex);
            throw;
        }
    }

    public IActionResult EditConfig()
    {
        if (HttpContext.Session.GetString("IsConfigAuthorized") != "true")
        {
            LogWarning("Попытка доступа к редактированию без авторизации");
            return RedirectToAction("Login");
        }

        try
        {
            var jsonObj = _appSettingsService.ReadConfig(_appSettingsPath);
            var comments = _appSettingsService.ReadComments();
            
            // Добавляем комментарии в основной объект для передачи в представление
            jsonObj["Comments"] = comments;
            
            return View(jsonObj);
        }
        catch (Exception ex)
        {
            LogError("Ошибка при загрузке конфигурации", ex);
            throw;
        }
    }

    [HttpPost]
    public IActionResult SaveConfig(IFormCollection form)
    {
        try
        {
            var updatedConfig = _appSettingsBuilder.BuildAppSettings(form);
            _appSettingsService.WriteConfig(updatedConfig, _appSettingsPath);
            LogInfo("Конфигурация успешно сохранена", new { Keys = form.Keys });
            return RedirectToAction("EditConfig");
        }
        catch (Exception ex)
        {
            LogError("Ошибка при сохранении конфигурации", ex);
            throw;
        }
    }

    public async Task<IActionResult> CoursesList()
    {
        if (!IsConfigAuthorized()) return RedirectToAction("Login");
        var courses = await _mediator.Send(new GetAllCoursesQuery());
        return View(courses);
    }

    public IActionResult CourseCreate()
    {
        if (!IsConfigAuthorized()) return RedirectToAction("Login");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CourseCreate(CreateCourseCommand command)
    {
        if (!IsConfigAuthorized()) return RedirectToAction("Login");
        if (!ModelState.IsValid) return View(command);
        await _mediator.Send(command);
        LogInfo("Курс создан через Config", command);
        return RedirectToAction("CoursesList");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CourseDelete(int id)
    {
        if (!IsConfigAuthorized()) return RedirectToAction("Login");
        await _mediator.Send(new DeleteCourseCommand(id));
        LogInfo("Курс удалён через Config", new { id });
        return RedirectToAction("CoursesList");
    }

    private bool IsConfigAuthorized() =>
        HttpContext.Session.GetString("IsConfigAuthorized") == "true";
}

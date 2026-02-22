using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlqStudio.Application.ApiClients.Moodle;
using SlqStudio.Application.CQRS.Course.Queries;
using SlqStudio.Application.Services;
using SlqStudio.Persistence.Models;
using SlqStudio.ViewModels.Auth;

namespace SlqStudio.Controllers
{
    public class AuthController : BaseMvcController
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IMediator _mediator;
        private readonly IMoodleService _moodleService;

        public AuthController(
            IJwtTokenService jwtTokenService,
            IMediator mediator,
            IMoodleService moodleService,
            ILogger<BaseMvcController> logger)
            : base(logger)
        {
            _jwtTokenService = jwtTokenService;
            _mediator = mediator;
            _moodleService = moodleService;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                LogInfo("Пользователь уже авторизован");
                return RedirectToLocal(returnUrl);
            }

            await LoadCoursesToViewBagAsync();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
        {
            await LoadCoursesToViewBagAsync();

            if (!ModelState.IsValid)
            {
                LogWarning("Неверная модель авторизации", request);
                TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля корректно.";
                return View(request);
            }

            try
            {
                var userData = await _moodleService.GetUserByEmailAsync(request.Email);
                if (userData == null)
                {
                    LogWarning("Пользователь не найден", request.Email);
                    TempData["ErrorMessage"] = "Пользователь с таким email не найден.";
                    return View(request);
                }

                if (string.IsNullOrEmpty(request.Course))
                {
                    LogWarning("Курс не найден", request.Course);
                    TempData["ErrorMessage"] = "Курс не выбран.";
                    return View(request);
                }

                var course = await _moodleService.GetAllCourseByName(request.Course);
                if (course == null)
                {
                    LogWarning("Курс не найден", request.Course);
                    TempData["ErrorMessage"] = "Выбранный курс не найден.";
                    return View(request);
                }

                var userRole = await _moodleService.GetUserProfileAsync(userData.Id, course.Id);
                if (userRole == null || userRole.Roles == null || !userRole.Roles.Any())
                {
                    LogWarning("Роль пользователя не найдена", new { userId = userData.Id });
                    TempData["ErrorMessage"] = "Не удалось определить вашу роль в системе.";
                    return View(request);
                }

                var tokenString = _jwtTokenService.GenerateJwtToken(
                    userData.Email,
                    userRole.Roles.FirstOrDefault()?.ShortName ?? string.Empty,
                    userData.FullName);

                Response.Cookies.Append("jwt", tokenString, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // разрешает отправку куки по HTTP
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                });
                LogInfo("Пользователь успешно авторизован", userData.Email);
                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                LogError("Ошибка при авторизации", ex);
                TempData["ErrorMessage"] = "Произошла ошибка при авторизации: " + ex.Message;
                return View(request);
            }
        }


        public IActionResult Logout()
        {
            LogInfo("Выход произошел успешно");
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login", "Auth");
        }

        private ActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        private async Task LoadCoursesToViewBagAsync()
        {
            try
            {
                var courses = await _mediator.Send(new GetAllCoursesQuery()) ?? new List<Course>();
                if (!courses.Any())
                {
                    TempData["ErrorMessage"] = "Не удалось загрузить список курсов. Пожалуйста, попробуйте позже.";
                }

                ViewBag.Courses = new SelectList(courses, "Name", "Name");
            }
            catch (Exception ex)
            {
                LogError("Ошибка при загрузке курсов", ex);
                TempData["ErrorMessage"] = "Не удалось загрузить список курсов. Проверьте подключение к базе данных.";
                ViewBag.Courses = new SelectList(Array.Empty<string>());
            }
        }

    }
}

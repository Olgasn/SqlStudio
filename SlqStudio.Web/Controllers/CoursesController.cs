using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SlqStudio.Application.CQRS.Course.Commands;
using SlqStudio.Application.CQRS.Course.Queries;

namespace SlqStudio.Controllers;

public class CoursesController : BaseMvcController
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator, ILogger<CoursesController> logger)
        : base(logger)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        LogInfo("Получение списка курсов");
        var courses = await _mediator.Send(new GetAllCoursesQuery());
        return View(courses);
    }

    public async Task<IActionResult> Details(int id)
    {
        LogInfo("Получение деталей курса", new { id });

        var course = await _mediator.Send(new GetCourseByIdQuery(id));
        if (course == null)
        {
            LogWarning("Курс не найден", new { id });
            return NotFound();
        }

        return View(course);
    }

    public IActionResult Create()
    {
        LogInfo("Открытие страницы создания курса");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseCommand command)
    {
        LogInfo("Создание курса", command);

        if (ModelState.IsValid)
        {
            await _mediator.Send(command);
            LogInfo("Курс успешно создан", command);
            return RedirectToAction(nameof(Index));
        }

        LogWarning("Ошибка валидации при создании курса", command);
        return View(command);
    }

    public async Task<IActionResult> Edit(int id)
    {
        LogInfo("Редактирование курса — загрузка", new { id });

        var course = await _mediator.Send(new GetCourseByIdQuery(id));
        if (course == null)
        {
            LogWarning("Курс не найден для редактирования", new { id });
            return NotFound();
        }

        var command = new UpdateCourseCommand(course.Id, course.Name, course.Description);
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCourseCommand command)
    {
        LogInfo("Редактирование курса — отправка", command);

        if (id != command.Id)
        {
            LogWarning("ID курса в URL не совпадает с ID в модели", new { id, commandId = command.Id });
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            await _mediator.Send(command);
            LogInfo("Курс успешно обновлён", command);
            return RedirectToAction(nameof(Index));
        }

        LogWarning("Ошибка валидации при редактировании курса", command);
        return View(command);
    }

    public async Task<IActionResult> Delete(int id)
    {
        LogInfo("Удаление курса — подтверждение", new { id });

        var course = await _mediator.Send(new GetCourseByIdQuery(id));
        if (course == null)
        {
            LogWarning("Курс не найден для удаления", new { id });
            return NotFound();
        }

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        LogInfo("Удаление курса — подтверждено", new { id });

        await _mediator.Send(new DeleteCourseCommand(id));
        LogInfo("Курс успешно удалён", new { id });

        return RedirectToAction(nameof(Index));
    }
}

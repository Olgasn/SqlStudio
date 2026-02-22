using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlqStudio.Application.CQRS.LabTask.Commands;
using SlqStudio.Application.CQRS.LabTask.Queries;
using SlqStudio.Application.CQRS.LabWork.Queries;
using SlqStudio.Application.Services.VariantServices;

namespace SlqStudio.Controllers;

public class LabTasksController : BaseMvcController
{
    private readonly IMediator _mediator;
    private readonly VariantServices _variantServices;

    public LabTasksController(ILogger<LabTasksController> logger, IMediator mediator, VariantServices variantServices)
        : base(logger)
    {
        _mediator = mediator;
        _variantServices = variantServices;
    }

    public async Task<IActionResult> Index()
    {
        LogInfo("Загрузка списка всех заданий");
        var tasks = await _mediator.Send(new GetAllTasksQuery());
        return View(tasks);
    }

    [Authorize(Roles = "editingteacher")]
    public async Task<IActionResult> Details(int id)
    {
        LogInfo($"Детали задания ID: {id}");
        var taskItem = await _mediator.Send(new GetTaskByIdQuery(id));
        if (taskItem == null)
        {
            LogWarning($"Задание с ID {id} не найдено");
            return NotFound();
        }
        return View(taskItem);
    }

    public async Task<IActionResult> DetailsByWork(int id)
    {
        LogInfo($"Генерация варианта по работе ID: {id}");
        var labWorkItem = await _mediator.Send(new GetLabWorkByIdQuery(id));
        if (labWorkItem == null)
        {
            LogWarning($"Лабораторная работа с ID {id} не найдена");
            return NotFound();
        }

        var email = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrWhiteSpace(email))
        {
            LogWarning("Невозможно сгенерировать вариант: UserEmail отсутствует в сессии");
            return Unauthorized();
        }

        return View(_variantServices.GenerateVariant(labWorkItem.Tasks.ToList(), email));
    }

    [Authorize(Roles = "editingteacher")]
    public async Task<IActionResult> Create()
    {
        LogInfo("Открытие формы создания задания");
        var labWorks = await _mediator.Send(new GetAllLabWorksQuery());
        ViewBag.LabWorks = new SelectList(labWorks, "Id", "Name");
        return View();
    }

    [Authorize(Roles = "editingteacher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskCommand command)
    {
        if (ModelState.IsValid)
        {
            LogInfo("Создание нового задания", command);
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }

        LogWarning("Модель недействительна при создании задания", ModelState);
        var labWorks = await _mediator.Send(new GetAllLabWorksQuery());
        ViewBag.LabWorks = new SelectList(labWorks, "Id", "Name");
        return View(command);
    }

    [Authorize(Roles = "editingteacher")]
    public async Task<IActionResult> Edit(int id)
    {
        LogInfo($"Открытие формы редактирования задания ID: {id}");
        var taskItem = await _mediator.Send(new GetTaskByIdQuery(id));
        if (taskItem == null)
        {
            LogWarning($"Задание с ID {id} не найдено для редактирования");
            return NotFound();
        }

        var command = new UpdateTaskCommand(taskItem.Id, taskItem.Number, taskItem.Title, taskItem.Condition, taskItem.SolutionExample, taskItem.LabWorkId);
        var labWorks = await _mediator.Send(new GetAllLabWorksQuery());
        ViewBag.LabWorks = new SelectList(labWorks, "Id", "Name", taskItem.LabWorkId);
        return View(command);
    }

    [Authorize(Roles = "editingteacher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTaskCommand command)
    {
        if (id != command.Id)
        {
            LogWarning($"Несовпадение ID при редактировании: путь ID = {id}, тело ID = {command.Id}");
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            LogInfo($"Обновление задания ID: {id}", command);
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }

        LogWarning("Модель недействительна при редактировании задания", ModelState);
        var labWorks = await _mediator.Send(new GetAllLabWorksQuery());
        ViewBag.LabWorks = new SelectList(labWorks, "Id", "Name");
        return View(command);
    }

    [Authorize(Roles = "editingteacher")]
    public async Task<IActionResult> Delete(int id)
    {
        LogInfo($"Запрос на удаление задания ID: {id}");
        var taskItem = await _mediator.Send(new GetTaskByIdQuery(id));
        if (taskItem == null)
        {
            LogWarning($"Задание с ID {id} не найдено для удаления");
            return NotFound();
        }
        return View(taskItem);
    }

    [Authorize(Roles = "editingteacher")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        LogInfo($"Подтверждено удаление задания ID: {id}");
        await _mediator.Send(new DeleteTaskCommand(id));
        return RedirectToAction(nameof(Index));
    }
}

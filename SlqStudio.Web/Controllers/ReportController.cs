using Application.Common.SQL.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SlqStudio.Application.CQRS.LabTask.Queries;
using SlqStudio.Application.CQRS.LabWork.Queries;
using SlqStudio.Application.Services.EmailService;
using SlqStudio.Application.Services.ReportBuider;
using SlqStudio.Application.Services.VariantServices;
using SlqStudio.DTO;
using SlqStudio.Persistence.Models;
using SlqStudio.Session;

namespace SlqStudio.Controllers;

public class ReportController : BaseMvcController
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly VariantServices _variantServices;

    public ReportController(IMediator mediator, 
                            IEmailService emailService,
                            VariantServices variantServices,
                            ILogger<ReportController> logger)
        : base(logger)
    {
        _mediator = mediator;
        _emailService = emailService;
        _variantServices = variantServices;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            LogInfo("Запрос на создание отчета.");
            var reportDto = await CreateReportDtoAsync();
            return View(reportDto);
        }
        catch (Exception ex)
        {
            LogError("Ошибка при создании отчета.", ex);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> SubmitReport(ReportDto report)
    {
        try
        {
            LogInfo("Запрос на отправку отчета.");
            var reportDto = await CreateReportDtoAsync();
            var director = new ReportDirector();
            string html = director.BuildHtmlReport(reportDto.User, reportDto.Solutions, reportDto.LabWorks);
            byte[] pdf = director.BuildPdfReport(reportDto.User, reportDto.Solutions, reportDto.LabWorks);

            var result = await _emailService.SendEmailAsync("evgbondarev@edu.gstu.by", reportDto.User.Email, html);
            LogInfo($"Отчет успешно отправлен на email {reportDto.User.Email}");
            return File(pdf, "application/pdf", "report.pdf");
        }
        catch (Exception ex)
        {
            LogError("Ошибка при отправке отчета.", ex);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private async Task<ReportDto> CreateReportDtoAsync()
    {
        try
        {
            LogInfo("Создание ReportDto...");
            var user = GetUserFromSession();
            var solutionResults = GetSolutionResultsFromSession();
            var variant = string.IsNullOrWhiteSpace(user.Email)
                ? new List<LabTask>()
                : _variantServices.GetVariantFromCache(user.Email);
            
            var labWorks = new List<LabWork>();

            foreach (var solutionResult in solutionResults)
            {
                var taskItem = await _mediator.Send(new GetTaskByIdQuery(solutionResult.TaskId));
                if (taskItem == null)
                {
                    LogWarning($"Задача с ID {solutionResult.TaskId} не найдена при формировании отчета.");
                    continue;
                }

                if (!labWorks.Any(l => l.Id == taskItem.LabWork.Id))
                {
                    var labWork = await _mediator.Send(new GetLabWorkByIdQuery(taskItem.LabWork.Id));
                    if (labWork == null)
                    {
                        LogWarning($"Лабораторная работа с ID {taskItem.LabWork.Id} не найдена при формировании отчета.");
                        continue;
                    }

                    labWork.Tasks = labWork.Tasks
                                            .Where(task => variant.Any(v => v.Id == task.Id))
                                            .ToList();
                    labWorks.Add(labWork);
                }
            }

            return new ReportDto
            {
                User = user,
                Solutions = solutionResults,
                LabWorks = labWorks
            };
        }
        catch (Exception ex)
        {
            LogError("Ошибка при создании ReportDto.", ex);
            throw;
        }
    }

    private UserDto GetUserFromSession()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        var name = HttpContext.Session.GetString("UserName");

        return new UserDto
        {
            Email = email ?? string.Empty,
            Name = name ?? string.Empty
        };
    }

    private List<SolutionResultDto> GetSolutionResultsFromSession()
    {
        var comparisonResults = HttpContext.Session.GetObjectFromJson<List<ComparisonResult>>("ComparisonResults") ?? new List<ComparisonResult>();

        return comparisonResults.Select(r => new SolutionResultDto
        {
            TaskId = r.TaskId,
            IsSuccess = r.Result,
            UserSolutionText = r.UserSolutionText
        }).ToList();
    }
}

using SlqStudio.Application.DTO;
using SlqStudio.Persistence.Models;

namespace SlqStudio.Application.Services.ReportBuider.Implementation;

public class ReportDirector
{
    public string BuildHtmlReport(UserDto user, List<SolutionResultDto> solutions, List<LabWork> labWorks)
    {
        var builder = new HtmlReportBuilder();
        return ((IHtmlReportBuilder)builder
                .AddUserInfo(user)
                .AddWorkInfo(solutions, labWorks)
                .AddSolutionDetails(solutions, labWorks))
            .Build();
    }

    public byte[] BuildPdfReport(UserDto user, List<SolutionResultDto> solutions, List<LabWork> labWorks)
    {
        using var builder = new PdfReportBuilder();
        return ((IPdfReportBuilder)builder
                .AddUserInfo(user)
                .AddWorkInfo(solutions, labWorks)
                .AddSolutionDetails(solutions, labWorks))
            .Build();
    }
}
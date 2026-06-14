using SlqStudio.Application.DTO;
using SlqStudio.Persistence.Models;

namespace SlqStudio.Application.Services.ReportBuider;

public interface IReportBuilder
{
    IReportBuilder AddUserInfo(UserDto user);
    IReportBuilder AddWorkInfo(List<SolutionResultDto> solutions, List<LabWork> labWorks);
    IReportBuilder AddSolutionDetails(List<SolutionResultDto> solutions, List<LabWork> labWorks);
}

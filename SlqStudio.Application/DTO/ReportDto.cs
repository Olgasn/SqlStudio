using SlqStudio.Persistence.Models;

namespace SlqStudio.DTO;

public class ReportDto
{
    public UserDto User { get; set; } = new();
    public List<SolutionResultDto> Solutions { get; set; } = new();
    public List<LabWork> LabWorks { get; set; } = new();
}

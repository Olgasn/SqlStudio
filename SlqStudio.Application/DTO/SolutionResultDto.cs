namespace SlqStudio.DTO;

public class SolutionResultDto
{
    public int TaskId { get; set; }
    public bool IsSuccess { get; set; }
    public string UserSolutionText { get; set; } = string.Empty;
}

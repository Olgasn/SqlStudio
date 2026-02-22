namespace Application.Common.SQL.ResponseModels;

public class ComparisonResult
{
    public int TaskId { get; set; }
    public bool Result { get; set; }
    public string UserSolutionText { get; set; } = string.Empty;
}

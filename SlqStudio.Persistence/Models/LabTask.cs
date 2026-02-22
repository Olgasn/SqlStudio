namespace SlqStudio.Persistence.Models;

public class LabTask
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string SolutionExample { get; set; } = string.Empty;
    
    public int LabWorkId { get; set; }
    public LabWork LabWork { get; set; } = null!;
}

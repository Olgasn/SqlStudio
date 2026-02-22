namespace SlqStudio.Persistence.Models;

public class LabWork
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Number { get; set; }
    
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    public ICollection<LabTask> Tasks { get; set; } = new List<LabTask>();
}

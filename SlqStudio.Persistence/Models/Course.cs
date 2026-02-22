namespace SlqStudio.Persistence.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<LabWork> LabWorks { get; set; } = new List<LabWork>();
}

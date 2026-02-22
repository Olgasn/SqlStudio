namespace SlqStudio.ViewModel.Models;

public class LabTaskViewModel
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public int LabWorkId { get; set; }
    public List<Dictionary<string, object>>? QueryData { get; set; }
    public string DatabaseDiagram { get; set; } = string.Empty;
}

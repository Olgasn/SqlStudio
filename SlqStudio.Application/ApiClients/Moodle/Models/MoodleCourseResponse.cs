namespace SlqStudio.Application.ApiClients.Moodle.Models;

public class MoodleCourseResponse
{
    public int Id { get; set; }
    public string Shortname { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string Displayname { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int StartDate { get; set; }
    public int EndDate { get; set; }
    public int Visible { get; set; }
}

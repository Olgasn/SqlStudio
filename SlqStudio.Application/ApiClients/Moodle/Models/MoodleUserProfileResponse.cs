namespace SlqStudio.Application.ApiClients.Moodle.Models;

public class MoodleUserProfileResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public List<MoodleUserRole> Roles { get; set; } = new List<MoodleUserRole>();
    public List<MoodleUserCourse> EnrolledCourses { get; set; } = new List<MoodleUserCourse>();
}

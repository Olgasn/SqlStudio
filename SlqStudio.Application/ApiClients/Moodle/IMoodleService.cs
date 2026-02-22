using SlqStudio.Application.ApiClients.Moodle.Models;

namespace SlqStudio.Application.ApiClients.Moodle;

public interface IMoodleService
{
    Task<MoodleUserProfileResponse?> GetUserProfileAsync(int userId, int courseId);
    Task<List<MoodleCourseResponse>> GetAllCoursesAsync();
    Task<MoodleCourseResponse?> GetAllCourseByName(string courseName);
    Task<MoodleUserProfileResponse?> GetUserByEmailAsync(string email);
}

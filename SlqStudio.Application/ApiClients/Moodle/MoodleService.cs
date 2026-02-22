using SlqStudio.Application.ApiClients.Moodle.Models;

namespace SlqStudio.Application.ApiClients.Moodle;

public class MoodleService : IMoodleService
{
    private readonly MoodleApiClient _moodleApiClient;

    public MoodleService(MoodleApiClient moodleApiClient)
    {
        _moodleApiClient = moodleApiClient;
    }

    public async Task<MoodleUserProfileResponse?> GetUserProfileAsync(int userId, int courseId)
    {
        var parameters = new Dictionary<string, string>
        {
            { "userlist[0][userid]", userId.ToString() },
            { "userlist[0][courseid]", courseId.ToString() }
        };
        return (await _moodleApiClient.SendRequestAsync<MoodleUserProfileResponse>(
            "core_user_get_course_user_profiles", parameters)).FirstOrDefault();
    }

    public async Task<List<MoodleCourseResponse>> GetAllCoursesAsync()
    {
        var parameters = new Dictionary<string, string>();
        return await _moodleApiClient.SendRequestAsync<MoodleCourseResponse>(
            "core_course_get_courses", parameters);
    }
    
    public async Task<MoodleCourseResponse?> GetAllCourseByName(string courseName)
    {
        var parameters = new Dictionary<string, string>();
        var allCourses = await _moodleApiClient.SendRequestAsync<MoodleCourseResponse>(
            "core_course_get_courses", parameters);
        return allCourses.FirstOrDefault(e => e.Displayname == courseName);
    }
    
    public async Task<MoodleUserProfileResponse?> GetUserByEmailAsync(string email)
    {
        var parameters = new Dictionary<string, string>
        {
            { "field", "email" },
            { "values[0]", email }
        };
        return (await _moodleApiClient.SendRequestAsync<MoodleUserProfileResponse>(
            "core_user_get_users_by_field", parameters)).FirstOrDefault();
    }
}

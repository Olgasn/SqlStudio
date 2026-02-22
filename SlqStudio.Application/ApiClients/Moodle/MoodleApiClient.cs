using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using SlqStudio.Application.ApiClients.Moodle.Models;

namespace SlqStudio.Application.ApiClients.Moodle;

public class MoodleApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly MoodleApiSettings _settings;

    public MoodleApiClient(IHttpClientFactory httpClientFactory, IOptions<MoodleApiSettings> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        
        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
    }

    public async Task<List<TResponse>> SendRequestAsync<TResponse>(string functionName, Dictionary<string, string> parameters)
    {
        var client = _httpClientFactory.CreateClient();
        var queryParams = new StringBuilder();
        queryParams.Append($"?wstoken={_settings.Token}");
        queryParams.Append($"&wsfunction={functionName}");
        queryParams.Append($"&moodlewsrestformat=json");
        
        foreach (var param in parameters)
        {
            queryParams.Append($"&{param.Key}={param.Value}");
        }

        HttpResponseMessage response = await _retryPolicy.ExecuteAsync(() => client.GetAsync(_settings.MoodleUrl + queryParams.ToString()));
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<TResponse>>(jsonResponse) ?? new List<TResponse>();
    }
}

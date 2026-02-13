using System.Net.Http;
using OpenShock.SDK.CSharp;

namespace VoiceShock;

public static class OpenShockClient
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new System.Uri("https://api.openshock.app/"),
        DefaultRequestHeaders =
        {
            { "User-Agent", "VoiceShock/1.0.0" },
            { "Accept", "application/json" }
        }
    };
    
    public static HttpClient HttpClient => _httpClient;
    
    public static OpenShockApiClient ApiClient { get; private set; } = new OpenShockApiClient(_httpClient);

    public static void Initialize(string token)
    {
        // Remove existing token header if present
        if (_httpClient.DefaultRequestHeaders.Contains("OpenShockToken"))
        {
            _httpClient.DefaultRequestHeaders.Remove("OpenShockToken");
        }
        
        _httpClient.DefaultRequestHeaders.Add("OpenShockToken", token);
        
        // Re-initialize ApiClient to ensure it uses the updated HttpClient state if necessary
        // although it should be fine since it's the same instance, but good practice.
        ApiClient = new OpenShockApiClient(_httpClient);
    }
}

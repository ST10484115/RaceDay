
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace RaceDay.MVC.Services;

public class RaceDayApiService : IRaceDayApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RaceDayApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthenticationCookie(HttpRequestMessage request)
    {
        var context = _httpContextAccessor.HttpContext;

        var cookie = context?.Session.GetString("ApiAuthCookie");

        if (!string.IsNullOrWhiteSpace(cookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }
    }

    private void SaveAuthenticationCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            throw new InvalidOperationException(
                "The API did not return an authentication cookie.");
        }

        var authCookie = cookies.FirstOrDefault(cookie =>
            cookie.StartsWith("RaceDay.Auth=", StringComparison.OrdinalIgnoreCase));

        if (authCookie == null)
        {
            throw new InvalidOperationException(
                "The API authentication cookie was not found.");
        }

        var cookieValue = authCookie.Split(';')[0];

        _httpContextAccessor.HttpContext?.Session.SetString(
            "ApiAuthCookie",
            cookieValue);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get, endpoint);

        AddAuthenticationCookie(request);

        using var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(data)
        };

        AddAuthenticationCookie(request);

        using var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        if (endpoint.Equals(
            "/api/Auth/login",
            StringComparison.OrdinalIgnoreCase))
        {
            SaveAuthenticationCookie(response);
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>();

        if (endpoint.Equals(
            "/api/Auth/logout",
            StringComparison.OrdinalIgnoreCase))
        {
            _httpContextAccessor.HttpContext?.Session.Remove("ApiAuthCookie");
            _httpContextAccessor.HttpContext?.Session.Remove("UserRole");
            _httpContextAccessor.HttpContext?.Session.Remove("UserId");
        }

        return result;
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put, endpoint)
        {
            Content = JsonContent.Create(data)
        };

        AddAuthenticationCookie(request);

        using var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete, endpoint);

        AddAuthenticationCookie(request);

        using var response = await _httpClient.SendAsync(request);

        return response.IsSuccessStatusCode;
    }
}

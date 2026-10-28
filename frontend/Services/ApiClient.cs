using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StudentClubManagement.Services;

/// <summary>Kết quả gọi API không có dữ liệu trả về.</summary>
public record ApiResult(bool Success, int StatusCode, string? Error)
{
    /// <summary>Đưa lỗi từ API vào ModelState để view hiển thị.</summary>
    public void AddTo(ModelStateDictionary modelState) { if (!Success && !string.IsNullOrEmpty(Error)) modelState.AddModelError("", Error); }
}

/// <summary>Kết quả gọi API có dữ liệu trả về.</summary>
public record ApiResult<T>(bool Success, int StatusCode, string? Error, T? Data) : ApiResult(Success, StatusCode, Error);

/// <summary>401/403/404 từ API; middleware sẽ chuyển thành đăng nhập lại / AccessDenied / 404.</summary>
public class ApiStatusException(HttpStatusCode status) : Exception($"API trả về {(int)status}") { public HttpStatusCode Status { get; } = status; }

/// <summary>
/// Client duy nhất để Frontend nói chuyện với Backend. Tự gắn JWT lấy từ cookie đăng nhập.
/// Các hàm Get* ném <see cref="ApiStatusException"/> với 401/403/404; mọi lỗi khác trả về trong ApiResult.
/// </summary>
public class ApiClient(HttpClient http, IHttpContextAccessor accessor)
{
    public const string TokenClaim = "access_token";
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { ReferenceHandler = ReferenceHandler.IgnoreCycles };

    /// <summary>Tạo query string từ các cặp (tên, giá trị); bỏ qua giá trị null/rỗng.</summary>
    public static string Url(string path, params (string Key, object? Value)[] query)
    {
        var parts = query.Where(x => x.Value is not null && !string.IsNullOrWhiteSpace(x.Value.ToString()))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(Convert.ToString(x.Value, System.Globalization.CultureInfo.InvariantCulture)!)}");
        var qs = string.Join("&", parts);
        return qs.Length == 0 ? path : $"{path}?{qs}";
    }

    /// <summary>GET và trả dữ liệu; ném ApiStatusException với 401/403/404, ném HttpRequestException với lỗi khác.</summary>
    public async Task<T> GetAsync<T>(string url)
    {
        using var response = await SendAsync(HttpMethod.Get, url, null);
        await ThrowForStatusAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    public async Task<ApiResult<T>> PostAsync<T>(string url, object? body = null) => await SendForResultAsync<T>(HttpMethod.Post, url, body);
    public async Task<ApiResult> PostAsync(string url, object? body = null) => await SendForResultAsync(HttpMethod.Post, url, body);
    public async Task<ApiResult> PutAsync(string url, object? body = null) => await SendForResultAsync(HttpMethod.Put, url, body);
    public async Task<ApiResult> DeleteAsync(string url) => await SendForResultAsync(HttpMethod.Delete, url, null);

    async Task<ApiResult> SendForResultAsync(HttpMethod method, string url, object? body)
    {
        using var response = await SendAsync(method, url, body);
        MapAuthStatus(response);
        return response.IsSuccessStatusCode ? new ApiResult(true, (int)response.StatusCode, null) : new ApiResult(false, (int)response.StatusCode, await ReadErrorAsync(response));
    }

    async Task<ApiResult<T>> SendForResultAsync<T>(HttpMethod method, string url, object? body)
    {
        using var response = await SendAsync(method, url, body);
        MapAuthStatus(response);
        if (!response.IsSuccessStatusCode) return new ApiResult<T>(false, (int)response.StatusCode, await ReadErrorAsync(response), default);
        var data = response.Content.Headers.ContentLength == 0 ? default : await response.Content.ReadFromJsonAsync<T>(Json);
        return new ApiResult<T>(true, (int)response.StatusCode, null, data);
    }

    async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);
        var token = accessor.HttpContext?.User.FindFirst(TokenClaim)?.Value;
        if (!string.IsNullOrEmpty(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body, options: Json);
        return await http.SendAsync(request);
    }

    // Đăng nhập sai trả 401 nhưng đó là lỗi nghiệp vụ, nên chỉ ném 401 khi đã có token (phiên hết hạn).
    void MapAuthStatus(HttpResponseMessage r)
    {
        var hasToken = !string.IsNullOrEmpty(accessor.HttpContext?.User.FindFirst(TokenClaim)?.Value);
        if ((r.StatusCode == HttpStatusCode.Unauthorized && hasToken) || r.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound) throw new ApiStatusException(r.StatusCode);
    }

    async Task ThrowForStatusAsync(HttpResponseMessage r)
    {
        if (r.IsSuccessStatusCode) return;
        if (r.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound) throw new ApiStatusException(r.StatusCode);
        throw new HttpRequestException(await ReadErrorAsync(r) ?? $"API trả về {(int)r.StatusCode}");
    }

    static async Task<string?> ReadErrorAsync(HttpResponseMessage r)
    {
        try
        {
            using var doc = JsonDocument.Parse(await r.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) return m.GetString();
            if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                return string.Join(" ", errs.EnumerateObject().SelectMany(p => p.Value.EnumerateArray().Select(e => e.GetString())));
            if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String) return d.GetString();
            if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String) return t.GetString();
        }
        catch (JsonException) { }
        return $"Yêu cầu không thành công ({(int)r.StatusCode}).";
    }
}


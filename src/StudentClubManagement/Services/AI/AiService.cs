using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudentClubManagement.Data;
using StudentClubManagement.Models;

namespace StudentClubManagement.Services;

public class AiService(ApplicationDbContext db, IHttpClientFactory clients, IConfiguration config, ILogger<AiService> logger) : IAiService
{
    public async Task<string> ChatAsync(ApplicationUser user, string question)
    {
        var member = await db.Members.FirstOrDefaultAsync(x => x.UserId == user.Id); if (member is null) return "Không đủ dữ liệu để trả lời.";
        var tasks = await db.TaskAssignments.Where(x => x.MemberId == member.Id).Select(x => new { x.Task.Title, x.Task.Progress, x.Task.DueDate }).Take(10).ToListAsync();
        var points = await db.PointHistory.Where(x => x.MemberId == member.Id).SumAsync(x => x.Points);
        var context = JsonSerializer.Serialize(new { member.FullName, member.StudentCode, Tasks = tasks, TotalPoints = points });
        return await CompleteAsync(user.Id, "CHATBOT", question, context) ?? $"Thông tin của bạn: {tasks.Count} nhiệm vụ, tổng điểm {points}.";
    }
    public async Task<IReadOnlyList<AiRecommendation>> RecommendAsync(ApplicationUser user, int taskId, int? clubId)
    {
        var task = await db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException(); var q = db.Members.Include(x => x.Skills).Where(x => x.MemberStatus == DomainStatus.Active);
        if (clubId != null) q = q.Where(x => x.ClubId == clubId);
        var candidates = await q.Select(x => new { x.Id, x.FullName, SkillCount = x.Skills.Count, Workload = db.TaskAssignments.Count(a => a.MemberId == x.Id && a.Task.Progress < 100) }).ToListAsync();
        var ranked = candidates.OrderBy(x => x.Workload).ThenByDescending(x => x.SkillCount).Take(5).Select((x, i) => new AiRecommendation(x.Id, x.FullName, Math.Max(50, 90 - i * 8), $"Có {x.SkillCount} kỹ năng; đang có {x.Workload} nhiệm vụ mở.")).ToList();
        await LogAsync(user.Id, "TASK_RECOMMENDATION", $"Task {task.Id}; {candidates.Count} ứng viên", JsonSerializer.Serialize(ranked), "FALLBACK"); return ranked;
    }
    public async Task<string> EvaluateParticipationAsync(ApplicationUser user, int memberId)
    {
        if (!await db.Members.AnyAsync(x => x.Id == memberId)) return "Không đủ dữ liệu để trả lời.";
        var events = await db.ActivityAttendance.CountAsync(x => x.MemberId == memberId && x.AttendanceStatus == "PRESENT");
        var meetings = await db.MeetingAttendance.CountAsync(x => x.MemberId == memberId && x.AttendanceStatus == "PRESENT");
        var completed = await db.TaskAssignments.CountAsync(x => x.MemberId == memberId && x.Task.Progress == 100);
        if (events + meetings + completed == 0) return "Không đủ dữ liệu để trả lời.";
        var result = $"Đánh giá hỗ trợ: {events} hoạt động, {meetings} cuộc họp và {completed} nhiệm vụ hoàn thành. Người quản lý phải xem xét trước mọi quyết định.";
        await LogAsync(user.Id, "PARTICIPATION_EVALUATION", $"Member {memberId}", result, "SUCCESS"); return result;
        // TODO(BUSINESS_RULE_CONFIRMATION): Chưa áp dụng ngưỡng hoặc khoảng thời gian đánh giá chính thức.
    }
    private async Task<string?> CompleteAsync(int userId, string function, string input, string context)
    {
        var baseUrl = config["Ai:BaseUrl"]; var key = config["AI_API_KEY"]; var model = config["Ai:Model"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(model)) { await LogAsync(userId, function, input[..Math.Min(200, input.Length)], "Provider chưa cấu hình", "FALLBACK"); return null; }
        try
        {
            var client = clients.CreateClient("AI"); client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            var body = JsonSerializer.Serialize(new { model, messages = new[] { new { role = "system", content = "Chỉ trả lời từ DATA. Nếu thiếu: Không đủ dữ liệu để trả lời." }, new { role = "user", content = $"DATA:{context}\nQUESTION:{input}" } } });
            using var response = await client.PostAsync(baseUrl, new StringContent(body, Encoding.UTF8, "application/json")); response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); var output = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            if (string.IsNullOrWhiteSpace(output)) throw new JsonException(); await LogAsync(userId, function, input[..Math.Min(200, input.Length)], output, "SUCCESS"); return output;
        }
        catch (Exception ex) { logger.LogWarning(ex, "AI provider failed for {Function}", function); await LogAsync(userId, function, input[..Math.Min(200, input.Length)], "AI service unavailable", "ERROR"); return "Dịch vụ AI tạm thời không khả dụng. Vui lòng thử lại sau."; }
    }
    private async Task LogAsync(int userId, string fn, string input, string output, string status) { db.AiLogs.Add(new() { UserId = userId, AiFunction = fn, InputSummary = input, OutputResult = output, ModelName = config["Ai:Model"] ?? "local-safe-fallback", Status = status }); await db.SaveChangesAsync(); }
}

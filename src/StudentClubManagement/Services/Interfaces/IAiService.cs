using StudentClubManagement.Models;

namespace StudentClubManagement.Services;

public record AiRecommendation(int MemberId, string MemberName, int Suitability, string Reason);
public interface IAiService { Task<string> ChatAsync(ApplicationUser user, string question); Task<IReadOnlyList<AiRecommendation>> RecommendAsync(ApplicationUser user, int taskId, int? clubId); Task<string> EvaluateParticipationAsync(ApplicationUser user, int memberId); }

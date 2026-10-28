using StudentClubManagement.Models;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Services;

public interface IAiService { Task<string> ChatAsync(ApplicationUser user, string question); Task<IReadOnlyList<AiRecommendation>> RecommendAsync(ApplicationUser user, int taskId, int? clubId); Task<string> EvaluateParticipationAsync(ApplicationUser user, int memberId); }

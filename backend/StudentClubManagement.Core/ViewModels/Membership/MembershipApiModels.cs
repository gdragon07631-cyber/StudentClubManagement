using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

/// <summary>Danh sách đơn đăng ký kèm danh sách câu lạc bộ để lọc.</summary>
public record ApplicationListResponse(List<MembershipApplication> Items, List<ClubOption> Clubs);

/// <summary>Kết quả tra cứu trạng thái đơn (Application = null nếu không có đơn).</summary>
public record ApplicationStatusResponse(MembershipApplication? Application);

/// <summary>Yêu cầu từ chối đơn đăng ký.</summary>
public record RejectApplicationRequest(string? Reason);

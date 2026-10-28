using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

/// <summary>Danh sách tài khoản, hồ sơ thành viên để liên kết và vai trò hiện tại của từng tài khoản.</summary>
public record UserListResponse(List<ApplicationUser> Users, List<Member> Members, Dictionary<int, string> CurrentRoles);

/// <summary>Yêu cầu đổi vai trò tài khoản.</summary>
public record SetRoleRequest(string Role, int? MemberId);

/// <summary>Kết quả đổi vai trò; IsCurrentUser = tài khoản bị đổi là người đang gọi.</summary>
public record SetRoleResponse(bool IsCurrentUser);

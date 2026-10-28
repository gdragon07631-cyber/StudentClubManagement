using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

/// <summary>Mục chọn (Id, tên) cho dropdown.</summary>
public record TaskLookupItem(int Id, string Name);

/// <summary>Dữ liệu trang danh sách nhiệm vụ.</summary>
public record TaskListResponse(List<ClubTask> Tasks, List<TaskLookupItem> Members);

/// <summary>Nguồn (hoạt động/cuộc họp) có thể gắn với nhiệm vụ.</summary>
public record TaskSourcesResponse(List<TaskLookupItem> Activities, List<TaskLookupItem> Meetings);

/// <summary>Dữ liệu sửa nhiệm vụ.</summary>
public record TaskEditResponse(ClubTask Task, TaskSourcesResponse Sources);

public record AssignTaskRequest(int MemberId);

public record UpdateTaskProgressRequest(int Progress);

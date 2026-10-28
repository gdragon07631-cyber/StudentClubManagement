using System.ComponentModel.DataAnnotations;
using StudentClubManagement.Models;

namespace StudentClubManagement.ViewModels;

/// <summary>Lựa chọn câu lạc bộ cho dropdown (hoạt động, cuộc họp).</summary>
public class ScheduleClubOption
{
    public int Id { get; set; }
    public string ClubName { get; set; } = "";
}

/// <summary>Yêu cầu ghi điểm danh cho một thành viên.</summary>
public class AttendanceInput
{
    public int MemberId { get; set; }
    public string Status { get; set; } = "";
}

/// <summary>Dữ liệu tạo/sửa hoạt động gửi lên API.</summary>
public class ActivityInput
{
    public int ClubId { get; set; }
    [Required, MaxLength(180)] public string ActivityName { get; set; } = "";
    [MaxLength(2000)] public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [MaxLength(300)] public string? Location { get; set; }
    [MaxLength(30)] public string? Status { get; set; }
    [MaxLength(2000)] public string? Result { get; set; }
}

/// <summary>Một hoạt động kèm điểm danh và danh sách thành viên chưa điểm danh.</summary>
public class ActivityCard
{
    public Activity Activity { get; set; } = null!;
    public List<ActivityAttendance> Attendances { get; set; } = [];
    public List<Member> Unrecorded { get; set; } = [];
}

/// <summary>Dữ liệu tạo/sửa cuộc họp gửi lên API.</summary>
public class MeetingInput
{
    public int ClubId { get; set; }
    [Required, MaxLength(180)] public string Title { get; set; } = "";
    public DateTime MeetingTime { get; set; }
    [MaxLength(300)] public string? Location { get; set; }
    [MaxLength(3000)] public string? Agenda { get; set; }
    [MaxLength(6000)] public string? Minutes { get; set; }
    [MaxLength(3000)] public string? Decisions { get; set; }
    [MaxLength(30)] public string? Status { get; set; }
}

/// <summary>Một cuộc họp kèm điểm danh và danh sách thành viên chưa điểm danh.</summary>
public class MeetingCard
{
    public Meeting Meeting { get; set; } = null!;
    public List<MeetingAttendance> Attendances { get; set; } = [];
    public List<Member> Unrecorded { get; set; } = [];
}

/// <summary>Dữ liệu ghi điểm hoạt động gửi lên API.</summary>
public class PointRecordInput
{
    public int MemberId { get; set; }
    public int PointRuleId { get; set; }
    [Required, MaxLength(30)] public string SourceType { get; set; } = "";
    public int SourceId { get; set; }
    public decimal Points { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = "";
}

public class PointMemberOption
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string StudentCode { get; set; } = "";
    public string? ClubName { get; set; }
}

/// <summary>Lịch sử điểm cùng danh sách thành viên/quy tắc cho form ghi điểm (chỉ Admin nhận danh sách).</summary>
public class PointIndexResponse
{
    public List<PointHistory> Items { get; set; } = [];
    public List<PointMemberOption> ActiveMembers { get; set; } = [];
    public List<PointRule> Rules { get; set; } = [];
}

using Microsoft.EntityFrameworkCore;
using StudentClubManagement.Data;
using StudentClubManagement.Models;

namespace StudentClubManagement.Services;

public class ClubService(ApplicationDbContext db) : IClubService
{
    public async Task ApproveApplicationAsync(int id, int reviewerId)
    {
        var item = await db.MembershipApplications.SingleAsync(x => x.Id == id && x.Status == DomainStatus.Pending);
        item.Status = DomainStatus.Approved; item.ReviewedById = reviewerId; item.ReviewedAt = DateTime.UtcNow;
        if (!await db.Members.AnyAsync(x => x.ClubId == item.ClubId && x.StudentCode == item.StudentCode))
            db.Members.Add(new Member { ClubId = item.ClubId, StudentCode = item.StudentCode, FullName = item.FullName, Email = item.Email, Phone = item.Phone, ClassName = item.ClassName, Course = item.Course });
        // Quản trị viên hoặc trưởng ban cấp tài khoản riêng sau khi duyệt đơn.
        await db.SaveChangesAsync();
    }
    public async Task RejectApplicationAsync(int id, int reviewerId, string reason)
    {
        var item = await db.MembershipApplications.SingleAsync(x => x.Id == id && x.Status == DomainStatus.Pending);
        item.Status = DomainStatus.Rejected; item.ReviewedById = reviewerId; item.ReviewedAt = DateTime.UtcNow; item.RejectionReason = reason; await db.SaveChangesAsync();
    }
    public async Task RegisterActivityAsync(int activityId, int memberId)
    {
        if (!await db.Activities.AnyAsync(a => a.Id == activityId && db.Members.Any(m => m.Id == memberId && m.ClubId == a.ClubId))) throw new InvalidOperationException("Hoạt động không thuộc câu lạc bộ của bạn.");
        if (await db.ActivityRegistrations.AnyAsync(x => x.ActivityId == activityId && x.MemberId == memberId)) throw new InvalidOperationException("Bạn đã đăng ký hoạt động này.");
        db.ActivityRegistrations.Add(new() { ActivityId = activityId, MemberId = memberId }); await db.SaveChangesAsync();
    }
    public async Task CancelActivityRegistrationAsync(int activityId, int memberId)
    { var item = await db.ActivityRegistrations.SingleAsync(x => x.ActivityId == activityId && x.MemberId == memberId); item.Status = "CANCELLED"; await db.SaveChangesAsync(); }
    public async Task RecordActivityAttendanceAsync(int activityId, int memberId, string status)
    { ValidateAttendance(status); if (!await db.Activities.AnyAsync(a => a.Id == activityId && db.Members.Any(m => m.Id == memberId && m.ClubId == a.ClubId))) throw new InvalidOperationException("Thành viên không thuộc câu lạc bộ của hoạt động."); if (await db.ActivityAttendance.AnyAsync(x => x.ActivityId == activityId && x.MemberId == memberId)) throw new InvalidOperationException("Đã có bản ghi điểm danh."); db.ActivityAttendance.Add(new() { ActivityId = activityId, MemberId = memberId, AttendanceStatus = status }); await db.SaveChangesAsync(); }
    public async Task RecordMeetingAttendanceAsync(int meetingId, int memberId, string status)
    { ValidateAttendance(status); if (!await db.Meetings.AnyAsync(meeting => meeting.Id == meetingId && db.Members.Any(member => member.Id == memberId && member.ClubId == meeting.ClubId))) throw new InvalidOperationException("Thành viên không thuộc câu lạc bộ của cuộc họp."); if (await db.MeetingAttendance.AnyAsync(x => x.MeetingId == meetingId && x.MemberId == memberId)) throw new InvalidOperationException("Đã có bản ghi điểm danh."); db.MeetingAttendance.Add(new() { MeetingId = meetingId, MemberId = memberId, AttendanceStatus = status }); await db.SaveChangesAsync(); }
    public async Task UpdateTaskProgressAsync(int taskId, int memberId, int progress)
    {
        if (progress is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(progress), "Tiến độ phải từ 0 đến 100.");
        if (!await db.TaskAssignments.AnyAsync(x => x.TaskId == taskId && x.MemberId == memberId)) throw new UnauthorizedAccessException();
        var task = await db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException(); task.Progress = progress; task.Status = progress == 100 ? "COMPLETED" : "IN_PROGRESS"; await db.SaveChangesAsync();
    }
    public async Task<decimal> BalanceAsync(DateTime? from = null, DateTime? to = null)
    { var q = db.FinancialTransactions.AsQueryable(); if (from != null) q = q.Where(x => x.TransactionDate >= from); if (to != null) q = q.Where(x => x.TransactionDate <= to); return await q.SumAsync(x => x.TransactionType == "INCOME" ? x.Amount : -x.Amount); }
    private static void ValidateAttendance(string status) { if (status is not ("PRESENT" or "ABSENT" or "LATE")) throw new ArgumentException("Trạng thái điểm danh không hợp lệ."); }
}

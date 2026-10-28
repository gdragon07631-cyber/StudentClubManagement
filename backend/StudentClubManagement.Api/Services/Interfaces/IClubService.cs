namespace StudentClubManagement.Services;

public interface IClubService
{
    Task ApproveApplicationAsync(int id, int reviewerId); Task RejectApplicationAsync(int id, int reviewerId, string reason);
    Task RegisterActivityAsync(int activityId, int memberId); Task CancelActivityRegistrationAsync(int activityId, int memberId);
    Task RecordActivityAttendanceAsync(int activityId, int memberId, string status); Task RecordMeetingAttendanceAsync(int meetingId, int memberId, string status);
    Task UpdateTaskProgressAsync(int taskId, int memberId, int progress); Task<decimal> BalanceAsync(DateTime? from = null, DateTime? to = null);
}


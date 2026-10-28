-- =============================================================================
-- HỆ THỐNG QUẢN LÝ CÂU LẠC BỘ SINH VIÊN CÓ TÍCH HỢP AI
-- File database đầy đủ: Schema (Bảng, Khóa ngoại, Index, Ràng buộc) + Dữ liệu thật
-- Tài khoản mẫu:
--   Admin       : admin / Admin@123
--   Trưởng CLB  : leader01 / Leader@123
--   Thành viên  : SV001 / Member@123
-- =============================================================================
IF DB_ID(N'StudentClubManagement') IS NULL
BEGIN
    CREATE DATABASE [StudentClubManagement];
END;
GO
USE [StudentClubManagement];
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [clubs] (
        [Id] int NOT NULL IDENTITY,
        [ClubName] nvarchar(160) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_clubs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [point_rules] (
        [Id] int NOT NULL IDENTITY,
        [RuleName] nvarchar(160) NOT NULL,
        [SourceType] nvarchar(30) NOT NULL,
        [PointValue] decimal(10,2) NOT NULL,
        [EffectiveFrom] datetime2 NOT NULL,
        [EffectiveTo] datetime2 NULL,
        [Status] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_point_rules] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [users] (
        [Id] int NOT NULL IDENTITY,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [activities] (
        [Id] int NOT NULL IDENTITY,
        [ClubId] int NOT NULL,
        [ActivityName] nvarchar(180) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [Location] nvarchar(300) NULL,
        [Status] nvarchar(30) NOT NULL,
        [Result] nvarchar(2000) NULL,
        CONSTRAINT [PK_activities] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_activities_dates] CHECK ([EndTime] >= [StartTime]),
        CONSTRAINT [FK_activities_clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [clubs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [meetings] (
        [Id] int NOT NULL IDENTITY,
        [ClubId] int NOT NULL,
        [Title] nvarchar(180) NOT NULL,
        [MeetingTime] datetime2 NOT NULL,
        [Location] nvarchar(300) NULL,
        [Agenda] nvarchar(3000) NULL,
        [Minutes] nvarchar(max) NULL,
        [Decisions] nvarchar(3000) NULL,
        [Status] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_meetings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_meetings_clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [clubs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [role_claims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_role_claims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_role_claims_roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [ai_logs] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [AiFunction] nvarchar(60) NOT NULL,
        [InputSummary] nvarchar(1000) NULL,
        [OutputResult] nvarchar(max) NULL,
        [ModelName] nvarchar(100) NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ai_logs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ai_logs_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [financial_transactions] (
        [Id] int NOT NULL IDENTITY,
        [ClubId] int NOT NULL,
        [TransactionType] nvarchar(10) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [TransactionDate] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        CONSTRAINT [PK_financial_transactions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_financial_amount] CHECK ([Amount] > 0),
        CONSTRAINT [FK_financial_transactions_clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [clubs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_financial_transactions_users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [membership_applications] (
        [Id] int NOT NULL IDENTITY,
        [ClubId] int NOT NULL,
        [StudentCode] nvarchar(30) NOT NULL,
        [FullName] nvarchar(160) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [ClassName] nvarchar(60) NULL,
        [Course] nvarchar(30) NULL,
        [AppliedAt] datetime2 NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ReviewedById] int NULL,
        [ReviewedAt] datetime2 NULL,
        [RejectionReason] nvarchar(500) NULL,
        CONSTRAINT [PK_membership_applications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_membership_applications_clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [clubs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_membership_applications_users_ReviewedById] FOREIGN KEY ([ReviewedById]) REFERENCES [users] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Title] nvarchar(180) NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [NotificationType] nvarchar(50) NOT NULL,
        [Channel] nvarchar(20) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_notifications_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [user_claims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_user_claims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_user_claims_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [user_logins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_user_logins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_user_logins_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [user_roles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_user_roles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_user_roles_roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_user_roles_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [user_tokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_user_tokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_user_tokens_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [tasks] (
        [Id] int NOT NULL IDENTITY,
        [ActivityId] int NULL,
        [MeetingId] int NULL,
        [Title] nvarchar(180) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [Priority] nvarchar(20) NOT NULL,
        [StartDate] datetime2 NULL,
        [DueDate] datetime2 NULL,
        [Status] nvarchar(30) NOT NULL,
        [Progress] int NOT NULL,
        CONSTRAINT [PK_tasks] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_tasks_progress] CHECK ([Progress] >= 0 AND [Progress] <= 100),
        CONSTRAINT [FK_tasks_activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [activities] ([Id]),
        CONSTRAINT [FK_tasks_meetings_MeetingId] FOREIGN KEY ([MeetingId]) REFERENCES [meetings] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [activity_attendance] (
        [Id] int NOT NULL IDENTITY,
        [ActivityId] int NOT NULL,
        [MemberId] int NOT NULL,
        [AttendanceStatus] nvarchar(20) NOT NULL,
        [CheckedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_activity_attendance] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_activity_attendance_activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [activities] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [activity_registrations] (
        [Id] int NOT NULL IDENTITY,
        [ActivityId] int NOT NULL,
        [MemberId] int NOT NULL,
        [RegisteredAt] datetime2 NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_activity_registrations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_activity_registrations_activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [activities] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [departments] (
        [Id] int NOT NULL IDENTITY,
        [ClubId] int NOT NULL,
        [DepartmentName] nvarchar(120) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [LeaderMemberId] int NULL,
        CONSTRAINT [PK_departments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_departments_clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [clubs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [members] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NULL,
        [ClubId] int NOT NULL,
        [DepartmentId] int NULL,
        [StudentCode] nvarchar(30) NOT NULL,
        [FullName] nvarchar(160) NOT NULL,
        [ClassName] nvarchar(60) NULL,
        [Course] nvarchar(30) NULL,
        [Position] nvarchar(80) NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(256) NOT NULL,
        [MemberStatus] nvarchar(30) NOT NULL,
        [JoinedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_members] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_members_clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [clubs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_members_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_members_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [meeting_attendance] (
        [Id] int NOT NULL IDENTITY,
        [MeetingId] int NOT NULL,
        [MemberId] int NOT NULL,
        [AttendanceStatus] nvarchar(20) NOT NULL,
        [CheckedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_meeting_attendance] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_meeting_attendance_meetings_MeetingId] FOREIGN KEY ([MeetingId]) REFERENCES [meetings] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_meeting_attendance_members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [members] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [member_skills] (
        [Id] int NOT NULL IDENTITY,
        [MemberId] int NOT NULL,
        [SkillName] nvarchar(100) NOT NULL,
        [SkillLevel] nvarchar(40) NULL,
        [Experience] nvarchar(1000) NULL,
        CONSTRAINT [PK_member_skills] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_member_skills_members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [members] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [point_history] (
        [Id] int NOT NULL IDENTITY,
        [MemberId] int NOT NULL,
        [PointRuleId] int NOT NULL,
        [SourceType] nvarchar(30) NOT NULL,
        [SourceId] int NOT NULL,
        [Points] decimal(10,2) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [RecordedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_point_history] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_point_history_members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [members] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_point_history_point_rules_PointRuleId] FOREIGN KEY ([PointRuleId]) REFERENCES [point_rules] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE TABLE [task_assignments] (
        [Id] int NOT NULL IDENTITY,
        [TaskId] int NOT NULL,
        [MemberId] int NOT NULL,
        [AssignedAt] datetime2 NOT NULL,
        [CompletionStatus] nvarchar(30) NOT NULL,
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_task_assignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_task_assignments_members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [members] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_task_assignments_tasks_TaskId] FOREIGN KEY ([TaskId]) REFERENCES [tasks] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_activities_ClubId_StartTime] ON [activities] ([ClubId], [StartTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_activities_Status] ON [activities] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_activity_attendance_ActivityId_MemberId] ON [activity_attendance] ([ActivityId], [MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_activity_attendance_MemberId] ON [activity_attendance] ([MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_activity_registrations_ActivityId_MemberId] ON [activity_registrations] ([ActivityId], [MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_activity_registrations_MemberId] ON [activity_registrations] ([MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ai_logs_UserId] ON [ai_logs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_departments_ClubId] ON [departments] ([ClubId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_departments_LeaderMemberId] ON [departments] ([LeaderMemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_financial_transactions_ClubId] ON [financial_transactions] ([ClubId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_financial_transactions_CreatedById] ON [financial_transactions] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_financial_transactions_TransactionDate] ON [financial_transactions] ([TransactionDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_financial_transactions_TransactionType_TransactionDate] ON [financial_transactions] ([TransactionType], [TransactionDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_meeting_attendance_MeetingId_MemberId] ON [meeting_attendance] ([MeetingId], [MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_meeting_attendance_MemberId] ON [meeting_attendance] ([MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_meetings_ClubId] ON [meetings] ([ClubId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_member_skills_MemberId_SkillName] ON [member_skills] ([MemberId], [SkillName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_members_ClubId_StudentCode] ON [members] ([ClubId], [StudentCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_members_DepartmentId] ON [members] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_members_MemberStatus] ON [members] ([MemberStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_members_UserId] ON [members] ([UserId]) WHERE [UserId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_membership_applications_ClubId] ON [membership_applications] ([ClubId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_membership_applications_ReviewedById] ON [membership_applications] ([ReviewedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_notifications_UserId_IsRead] ON [notifications] ([UserId], [IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_point_history_MemberId_RecordedAt] ON [point_history] ([MemberId], [RecordedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_point_history_PointRuleId] ON [point_history] ([PointRuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_role_claims_RoleId] ON [role_claims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_task_assignments_MemberId] ON [task_assignments] ([MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_task_assignments_TaskId_MemberId] ON [task_assignments] ([TaskId], [MemberId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_tasks_ActivityId_Status] ON [tasks] ([ActivityId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_tasks_DueDate] ON [tasks] ([DueDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_tasks_MeetingId] ON [tasks] ([MeetingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_user_claims_UserId] ON [user_claims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_user_logins_UserId] ON [user_logins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [IX_user_roles_RoleId] ON [user_roles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [users] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_users_Email] ON [users] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    ALTER TABLE [activity_attendance] ADD CONSTRAINT [FK_activity_attendance_members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [members] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    ALTER TABLE [activity_registrations] ADD CONSTRAINT [FK_activity_registrations_members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [members] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    ALTER TABLE [departments] ADD CONSTRAINT [FK_departments_members_LeaderMemberId] FOREIGN KEY ([LeaderMemberId]) REFERENCES [members] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908065209_InitialSqlServer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908065209_InitialSqlServer', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908184455_AddMembershipRegistrationDetails'
)
BEGIN
    ALTER TABLE [membership_applications] ADD [BirthYear] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908184455_AddMembershipRegistrationDetails'
)
BEGIN
    ALTER TABLE [membership_applications] ADD [Expectations] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908184455_AddMembershipRegistrationDetails'
)
BEGIN
    ALTER TABLE [membership_applications] ADD [Interests] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908184455_AddMembershipRegistrationDetails'
)
BEGIN
    ALTER TABLE [membership_applications] ADD [JoinReason] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908184455_AddMembershipRegistrationDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908184455_AddMembershipRegistrationDetails', N'10.0.11');
END;

COMMIT;
GO


-- =============================================================================
-- DỮ LIỆU THẬT TRÍCH XUẤT TỪ DATABASE StudentClubManagement
-- =============================================================================
SET NOCOUNT ON;

-- Bảng [dbo].[roles] (3 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[roles])
BEGIN
    SET IDENTITY_INSERT [dbo].[roles] ON;
    INSERT INTO [dbo].[roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (1, N'ADMIN', N'ADMIN', N'0658a02e-d46c-4279-b6e5-9878af84cb2d');
    INSERT INTO [dbo].[roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (2, N'CLUB_LEADER', N'CLUB_LEADER', N'172ca409-bb6c-49d1-9b6e-f6bf0da4245d');
    INSERT INTO [dbo].[roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (3, N'MEMBER', N'MEMBER', N'b3adf080-8888-4609-b892-5d2534126ea4');
    SET IDENTITY_INSERT [dbo].[roles] OFF;
END;
GO

-- Bảng [dbo].[users] (3 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[users])
BEGIN
    SET IDENTITY_INSERT [dbo].[users] ON;
    INSERT INTO [dbo].[users] ([Id], [Status], [CreatedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (1, N'ACTIVE', '2026-09-08T06:52:47.5187082', N'admin', N'ADMIN', N'admin@club.demo', N'ADMIN@CLUB.DEMO', 1, N'AQAAAAIAAYagAAAAEHTnr1tI33YkFdzeEWV9hEv46rKLlcM14yqGBAfHBfegiRF17B9pYlinfP+R11PuvA==', N'FKBJAGNBEERN5JWHIE7DAF344Y4ER6UG', N'5c06c048-f9c5-46fb-9037-9e6683841c01', NULL, 0, 0, NULL, 1, 0);
    INSERT INTO [dbo].[users] ([Id], [Status], [CreatedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (2, N'ACTIVE', '2026-09-08T06:52:47.7886497', N'leader01', N'LEADER01', N'leader@club.demo', N'LEADER@CLUB.DEMO', 1, N'AQAAAAIAAYagAAAAEBLIlMAjNLRVNzWZBmY7rrzj7oob90nRWzC5VF/RZn49LuqFADgDgKy+xQkQF2Ia2A==', N'VU5HRPDD35H2VMGRANF4QN2UX5OM46LJ', N'0e36c18c-7b2a-49b5-ab93-98808901ac62', NULL, 0, 0, NULL, 1, 0);
    INSERT INTO [dbo].[users] ([Id], [Status], [CreatedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (3, N'ACTIVE', '2026-09-08T06:52:47.8632189', N'SV001', N'SV001', N'member@club.demo', N'MEMBER@CLUB.DEMO', 1, N'AQAAAAIAAYagAAAAEFXEUY+ES50YRVSwaXuWXpz5Ckk9OHAjUyGQzaCJ9nLf5aCumSREUeuiX8XbNDWP9Q==', N'RCXE5HEBOIDDYYVWMC6JVNSX42T7UQKX', N'b6b34c2e-be18-475a-8a44-94267686efcf', NULL, 0, 0, NULL, 1, 0);
    SET IDENTITY_INSERT [dbo].[users] OFF;
END;
GO

-- Bảng [dbo].[user_roles] (3 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[user_roles])
BEGIN
    INSERT INTO [dbo].[user_roles] ([UserId], [RoleId]) VALUES (1, 1);
    INSERT INTO [dbo].[user_roles] ([UserId], [RoleId]) VALUES (2, 2);
    INSERT INTO [dbo].[user_roles] ([UserId], [RoleId]) VALUES (3, 3);
END;
GO

-- Bảng [dbo].[clubs] (2 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[clubs])
BEGIN
    SET IDENTITY_INSERT [dbo].[clubs] ON;
    INSERT INTO [dbo].[clubs] ([Id], [ClubName], [Description], [CreatedAt]) VALUES (1, N'CLB Công nghệ Sinh viên', N'Dữ liệu giả phục vụ trình diễn đồ án.', '2026-09-08T06:52:47.4531093');
    INSERT INTO [dbo].[clubs] ([Id], [ClubName], [Description], [CreatedAt]) VALUES (2, N'CLB Kỹ năng Sinh viên', N'Câu lạc bộ thứ hai dùng để minh họa phạm vi quản lý theo ClubId.', '2026-10-28T05:40:25.2294791');
    SET IDENTITY_INSERT [dbo].[clubs] OFF;
END;
GO

-- Bảng [dbo].[departments] (2 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[departments])
BEGIN
    SET IDENTITY_INSERT [dbo].[departments] ON;
    INSERT INTO [dbo].[departments] ([Id], [ClubId], [DepartmentName], [Description], [LeaderMemberId]) VALUES (1, 1, N'Ban Truyền thông', N'Phụ trách truyền thông.', NULL);
    INSERT INTO [dbo].[departments] ([Id], [ClubId], [DepartmentName], [Description], [LeaderMemberId]) VALUES (2, 1, N'Ban Sự kiện', N'Phụ trách tổ chức hoạt động.', NULL);
    SET IDENTITY_INSERT [dbo].[departments] OFF;
END;
GO

-- Bảng [dbo].[members] (5 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[members])
BEGIN
    SET IDENTITY_INSERT [dbo].[members] ON;
    INSERT INTO [dbo].[members] ([Id], [UserId], [ClubId], [DepartmentId], [StudentCode], [FullName], [ClassName], [Course], [Position], [Phone], [Email], [MemberStatus], [JoinedAt]) VALUES (1, 2, 1, 2, N'SVL001', N'Trưởng ban Demo', N'CNTT-K20', N'K20', N'Trưởng ban', NULL, N'leader@club.demo', N'ACTIVE', '2026-09-08T06:52:47.9285082');
    INSERT INTO [dbo].[members] ([Id], [UserId], [ClubId], [DepartmentId], [StudentCode], [FullName], [ClassName], [Course], [Position], [Phone], [Email], [MemberStatus], [JoinedAt]) VALUES (2, 3, 1, 2, N'SV001', N'Thành viên Demo', N'CNTT-K21', N'K21', N'Thành viên', NULL, N'member@club.demo', N'ACTIVE', '2026-09-08T06:52:47.9287426');
    INSERT INTO [dbo].[members] ([Id], [UserId], [ClubId], [DepartmentId], [StudentCode], [FullName], [ClassName], [Course], [Position], [Phone], [Email], [MemberStatus], [JoinedAt]) VALUES (3, NULL, 1, NULL, N'SVCRUD01', N'CRUD Updated Member', N'CNTT-K22A', N'K22', N'Member', N'0900000002', N'crud.updated@club.demo', N'INACTIVE', '2026-09-08T06:56:08.3184685');
    INSERT INTO [dbo].[members] ([Id], [UserId], [ClubId], [DepartmentId], [StudentCode], [FullName], [ClassName], [Course], [Position], [Phone], [Email], [MemberStatus], [JoinedAt]) VALUES (5, NULL, 1, NULL, N'dtc245200872', N'Luc Thanh Do', N'CNTT K23K', N'K23', NULL, N'0329479384', N'gdragon07631@gmail.com', N'ACTIVE', '2026-09-08T11:23:25.4592752');
    INSERT INTO [dbo].[members] ([Id], [UserId], [ClubId], [DepartmentId], [StudentCode], [FullName], [ClassName], [Course], [Position], [Phone], [Email], [MemberStatus], [JoinedAt]) VALUES (6, NULL, 2, NULL, N'SV002', N'Thành viên CLB Kỹ năng', N'QTKD-K21', N'K21', N'Thành viên', NULL, N'member2@club.demo', N'ACTIVE', '2026-10-28T05:40:25.2919999');
    SET IDENTITY_INSERT [dbo].[members] OFF;
END;
GO

-- Bảng [dbo].[member_skills] (2 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[member_skills])
BEGIN
    SET IDENTITY_INSERT [dbo].[member_skills] ON;
    INSERT INTO [dbo].[member_skills] ([Id], [MemberId], [SkillName], [SkillLevel], [Experience]) VALUES (1, 2, N'Thiết kế', N'Khá', N'Poster sự kiện');
    INSERT INTO [dbo].[member_skills] ([Id], [MemberId], [SkillName], [SkillLevel], [Experience]) VALUES (2, 1, N'Tổ chức', N'Tốt', N'Điều phối hoạt động');
    SET IDENTITY_INSERT [dbo].[member_skills] OFF;
END;
GO

-- Bảng [dbo].[activities] (2 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[activities])
BEGIN
    SET IDENTITY_INSERT [dbo].[activities] ON;
    INSERT INTO [dbo].[activities] ([Id], [ClubId], [ActivityName], [Description], [StartTime], [EndTime], [Location], [Status], [Result]) VALUES (1, 1, N'Ngày hội chào tân sinh viên', N'Hoạt động demo', '2026-09-15T08:00:00.0000000', '2026-09-15T17:00:00.0000000', N'Hội trường', N'PLANNED', NULL);
    INSERT INTO [dbo].[activities] ([Id], [ClubId], [ActivityName], [Description], [StartTime], [EndTime], [Location], [Status], [Result]) VALUES (2, 2, N'Workshop kỹ năng thuyết trình', N'Dữ liệu mẫu của câu lạc bộ thứ hai', '2026-11-07T08:00:00.0000000', '2026-11-07T11:00:00.0000000', N'Phòng học A2', N'PLANNED', NULL);
    SET IDENTITY_INSERT [dbo].[activities] OFF;
END;
GO

-- Bảng [dbo].[meetings] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[meetings])
BEGIN
    SET IDENTITY_INSERT [dbo].[meetings] ON;
    INSERT INTO [dbo].[meetings] ([Id], [ClubId], [Title], [MeetingTime], [Location], [Agenda], [Minutes], [Decisions], [Status]) VALUES (1, 1, N'Họp chuẩn bị sự kiện', '2026-09-10T19:00:00.0000000', N'Phòng CLB', N'Phân công và rà soát tiến độ', NULL, NULL, N'SCHEDULED');
    SET IDENTITY_INSERT [dbo].[meetings] OFF;
END;
GO

-- Bảng [dbo].[tasks] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[tasks])
BEGIN
    SET IDENTITY_INSERT [dbo].[tasks] ON;
    INSERT INTO [dbo].[tasks] ([Id], [ActivityId], [MeetingId], [Title], [Description], [Priority], [StartDate], [DueDate], [Status], [Progress]) VALUES (1, 1, NULL, N'Thiết kế poster', N'Poster truyền thông', N'HIGH', '2026-09-08T00:00:00.0000000', '2026-09-13T00:00:00.0000000', N'IN_PROGRESS', 25);
    SET IDENTITY_INSERT [dbo].[tasks] OFF;
END;
GO

-- Bảng [dbo].[task_assignments] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[task_assignments])
BEGIN
    SET IDENTITY_INSERT [dbo].[task_assignments] ON;
    INSERT INTO [dbo].[task_assignments] ([Id], [TaskId], [MemberId], [AssignedAt], [CompletionStatus], [CompletedAt]) VALUES (1, 1, 2, '2026-09-08T06:52:48.0625359', N'ASSIGNED', NULL);
    SET IDENTITY_INSERT [dbo].[task_assignments] OFF;
END;
GO

-- Bảng [dbo].[activity_registrations] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[activity_registrations])
BEGIN
    SET IDENTITY_INSERT [dbo].[activity_registrations] ON;
    INSERT INTO [dbo].[activity_registrations] ([Id], [ActivityId], [MemberId], [RegisteredAt], [Status]) VALUES (1, 1, 2, '2026-09-08T06:52:48.0732666', N'REGISTERED');
    SET IDENTITY_INSERT [dbo].[activity_registrations] OFF;
END;
GO

-- Bảng [dbo].[activity_attendance] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[activity_attendance])
BEGIN
    SET IDENTITY_INSERT [dbo].[activity_attendance] ON;
    INSERT INTO [dbo].[activity_attendance] ([Id], [ActivityId], [MemberId], [AttendanceStatus], [CheckedAt]) VALUES (1, 1, 1, N'PRESENT', '2026-09-08T06:52:48.1035465');
    SET IDENTITY_INSERT [dbo].[activity_attendance] OFF;
END;
GO

-- Bảng [dbo].[meeting_attendance] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[meeting_attendance])
BEGIN
    SET IDENTITY_INSERT [dbo].[meeting_attendance] ON;
    INSERT INTO [dbo].[meeting_attendance] ([Id], [MeetingId], [MemberId], [AttendanceStatus], [CheckedAt]) VALUES (1, 1, 2, N'PRESENT', '2026-09-08T06:52:48.1101219');
    SET IDENTITY_INSERT [dbo].[meeting_attendance] OFF;
END;
GO

-- Bảng [dbo].[point_rules] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[point_rules])
BEGIN
    SET IDENTITY_INSERT [dbo].[point_rules] ON;
    INSERT INTO [dbo].[point_rules] ([Id], [RuleName], [SourceType], [PointValue], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (1, N'Điểm demo tham gia hoạt động', N'ACTIVITY', 10.00, '2026-09-08T00:00:00.0000000', NULL, N'ACTIVE');
    SET IDENTITY_INSERT [dbo].[point_rules] OFF;
END;
GO

-- Bảng [dbo].[point_history] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[point_history])
BEGIN
    SET IDENTITY_INSERT [dbo].[point_history] ON;
    INSERT INTO [dbo].[point_history] ([Id], [MemberId], [PointRuleId], [SourceType], [SourceId], [Points], [Reason], [RecordedAt]) VALUES (1, 2, 1, N'ACTIVITY', 1, 10.00, N'Dữ liệu demo - không phải công thức chính thức', '2026-09-08T06:52:48.1545614');
    SET IDENTITY_INSERT [dbo].[point_history] OFF;
END;
GO

-- Bảng [dbo].[financial_transactions] (2 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[financial_transactions])
BEGIN
    SET IDENTITY_INSERT [dbo].[financial_transactions] ON;
    INSERT INTO [dbo].[financial_transactions] ([Id], [ClubId], [TransactionType], [Category], [Amount], [Description], [TransactionDate], [CreatedById]) VALUES (1, 1, N'INCOME', N'Quỹ', 5000000.00, N'Quỹ demo', '2026-09-08T00:00:00.0000000', 1);
    INSERT INTO [dbo].[financial_transactions] ([Id], [ClubId], [TransactionType], [Category], [Amount], [Description], [TransactionDate], [CreatedById]) VALUES (2, 1, N'EXPENSE', N'Sự kiện', 750000.00, N'Vật dụng demo', '2026-09-08T00:00:00.0000000', 1);
    SET IDENTITY_INSERT [dbo].[financial_transactions] OFF;
END;
GO

-- Bảng [dbo].[membership_applications] (3 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[membership_applications])
BEGIN
    SET IDENTITY_INSERT [dbo].[membership_applications] ON;
    INSERT INTO [dbo].[membership_applications] ([Id], [ClubId], [StudentCode], [FullName], [Email], [Phone], [ClassName], [Course], [AppliedAt], [Status], [ReviewedById], [ReviewedAt], [RejectionReason], [BirthYear], [Expectations], [Interests], [JoinReason]) VALUES (1, 1, N'SVNEW01', N'Sinh viên Đăng ký', N'applicant@demo.local', NULL, N'CNTT-K22', N'K22', '2026-09-08T06:52:48.1724519', N'PENDING', NULL, NULL, NULL, NULL, NULL, NULL, NULL);
    INSERT INTO [dbo].[membership_applications] ([Id], [ClubId], [StudentCode], [FullName], [Email], [Phone], [ClassName], [Course], [AppliedAt], [Status], [ReviewedById], [ReviewedAt], [RejectionReason], [BirthYear], [Expectations], [Interests], [JoinReason]) VALUES (2, 1, N'SVNEW02', N'Ứng viên Demo', N'applicant2@demo.local', NULL, NULL, NULL, '2026-09-08T06:52:48.1725900', N'REJECTED', NULL, NULL, N'Dữ liệu minh họa', NULL, NULL, NULL, NULL);
    INSERT INTO [dbo].[membership_applications] ([Id], [ClubId], [StudentCode], [FullName], [Email], [Phone], [ClassName], [Course], [AppliedAt], [Status], [ReviewedById], [ReviewedAt], [RejectionReason], [BirthYear], [Expectations], [Interests], [JoinReason]) VALUES (3, 1, N'dtc245200872', N'Luc Thanh Do', N'gdragon07631@gmail.com', N'0329479384', N'CNTT K23K', N'K23', '2026-09-08T11:21:43.9427807', N'APPROVED', 1, '2026-09-08T11:23:25.4357250', NULL, NULL, NULL, NULL, NULL);
    SET IDENTITY_INSERT [dbo].[membership_applications] OFF;
END;
GO

-- Bảng [dbo].[notifications] (1 dòng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[notifications])
BEGIN
    SET IDENTITY_INSERT [dbo].[notifications] ON;
    INSERT INTO [dbo].[notifications] ([Id], [UserId], [Title], [Content], [NotificationType], [Channel], [IsRead], [CreatedAt]) VALUES (1, 3, N'Nhiệm vụ mới', N'Bạn được giao thiết kế poster.', N'TASK', N'IN_APP', 0, '2026-09-08T06:52:48.1856602');
    SET IDENTITY_INSERT [dbo].[notifications] OFF;
END;
GO

UPDATE [dbo].[departments] SET [LeaderMemberId] = 1 WHERE [Id] = 2;

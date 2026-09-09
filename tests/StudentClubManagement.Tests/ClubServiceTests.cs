using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;

namespace StudentClubManagement.Tests;

public class ClubServiceTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly ApplicationDbContext db;
    private readonly ClubService service;
    public ClubServiceTests()
    {
        connection.Open(); db = new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options); db.Database.EnsureCreated(); service = new(db);
        db.Clubs.Add(new() { Id = 1, ClubName = "Test" }); db.Members.Add(new() { Id = 1, ClubId = 1, StudentCode = "T01", FullName = "Test Member", Email = "test@example.local" }); db.Activities.Add(new() { Id = 1, ClubId = 1, ActivityName = "Test", StartTime = DateTime.Today, EndTime = DateTime.Today.AddHours(1) }); db.SaveChanges();
    }
    [Fact]
    public async Task DuplicateActivityRegistrationIsRejected()
    { await service.RegisterActivityAsync(1, 1); await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterActivityAsync(1, 1)); }
    [Fact]
    public async Task MemberCannotRegisterActivityFromAnotherClub()
    {
        db.Clubs.Add(new() { Id = 2, ClubName = "Other Club" });
        db.Activities.Add(new() { Id = 2, ClubId = 2, ActivityName = "Other Activity", StartTime = DateTime.Today, EndTime = DateTime.Today.AddHours(1) });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterActivityAsync(2, 1));
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ProgressOutsideRangeIsRejected(int progress)
    { await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.UpdateTaskProgressAsync(1, 1, progress)); }
    [Fact]
    public async Task BalanceEqualsIncomeMinusExpense()
    {
        db.Users.Add(new ApplicationUser { Id = 1, UserName = "admin", NormalizedUserName = "ADMIN", Email = "a@b.local", NormalizedEmail = "A@B.LOCAL", PasswordHash = "not-used" });
        db.FinancialTransactions.AddRange(new FinancialTransaction { ClubId = 1, CreatedById = 1, Category = "Income", TransactionType = "INCOME", Amount = 100, TransactionDate = DateTime.Today }, new FinancialTransaction { ClubId = 1, CreatedById = 1, Category = "Expense", TransactionType = "EXPENSE", Amount = 35, TransactionDate = DateTime.Today }); await db.SaveChangesAsync();
        Assert.Equal(65, await service.BalanceAsync());
    }
    [Fact]
    public async Task ApprovingApplicationCreatesMemberButNotAccount()
    {
        db.Users.Add(new ApplicationUser { Id = 2, UserName = "reviewer", NormalizedUserName = "REVIEWER", Email = "r@b.local", NormalizedEmail = "R@B.LOCAL", PasswordHash = "not-used" });
        db.MembershipApplications.Add(new() { Id = 1, ClubId = 1, StudentCode = "NEW01", FullName = "New Student", Email = "new@example.local" }); await db.SaveChangesAsync();
        await service.ApproveApplicationAsync(1, 2); Assert.True(await db.Members.AnyAsync(x => x.StudentCode == "NEW01")); Assert.False(await db.Users.AnyAsync(x => x.UserName == "NEW01"));
    }
    [Fact]
    public async Task DemoSeedCreatesAllRolesAndAccounts()
    {
        using var seedConnection = new SqliteConnection("Data Source=:memory:"); seedConnection.Open();
        var collection = new ServiceCollection(); collection.AddLogging();
        collection.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(seedConnection));
        collection.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<int>>().AddEntityFrameworkStores<ApplicationDbContext>();
        using var provider = collection.BuildServiceProvider();
        await provider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
        await DemoSeed.SeedAsync(provider);
        var manager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.NotNull(await manager.FindByNameAsync("admin")); Assert.NotNull(await manager.FindByNameAsync("leader01")); Assert.NotNull(await manager.FindByNameAsync("SV001"));
        Assert.Equal(3, await provider.GetRequiredService<ApplicationDbContext>().Roles.CountAsync());
        Assert.Equal(2, await provider.GetRequiredService<ApplicationDbContext>().Clubs.CountAsync());
        Assert.NotNull(await provider.GetRequiredService<ApplicationDbContext>().Members.SingleAsync(x => x.UserId != null && x.Position == "Trưởng câu lạc bộ"));
    }
    public void Dispose() { db.Dispose(); connection.Dispose(); GC.SuppressFinalize(this); }
}

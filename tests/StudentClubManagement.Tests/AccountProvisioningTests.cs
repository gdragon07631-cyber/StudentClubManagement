using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;
using StudentClubManagement.ViewModels;

namespace StudentClubManagement.Tests;

public class AccountProvisioningTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly ServiceProvider provider;
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> users;
    private readonly AccountProvisioningService service;

    public AccountProvisioningTests()
    {
        connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connection));
        services.AddIdentityCore<ApplicationUser>(o => { o.User.RequireUniqueEmail = true; o.Password.RequireNonAlphanumeric = false; o.Password.RequiredLength = 8; })
            .AddRoles<IdentityRole<int>>().AddEntityFrameworkStores<ApplicationDbContext>();
        provider = services.BuildServiceProvider();
        db = provider.GetRequiredService<ApplicationDbContext>();
        users = provider.GetRequiredService<UserManager<ApplicationUser>>();
        db.Database.EnsureCreated();
        db.Clubs.Add(new Club { Id = 1, ClubName = "Test" });
        db.Roles.Add(new IdentityRole<int> { Name = AppRoles.Member, NormalizedName = AppRoles.Member.ToUpperInvariant() });
        db.Members.Add(new Member { ClubId = 1, StudentCode = "SVTEST", FullName = "Student", Email = "student@example.test" });
        db.MembershipApplications.Add(new MembershipApplication { Id = 1, ClubId = 1, StudentCode = "SVTEST", FullName = "Student", Email = "student@example.test", Status = DomainStatus.Approved });
        db.SaveChanges();
        service = new(db, users);
    }

    [Fact]
    public async Task ApprovedStudentGetsLinkedAccountWithMemberRoleAndHashedPassword()
    {
        await service.IssueAsync(1, "SVTEST", "Student123");
        var user = await users.FindByNameAsync("SVTEST");
        Assert.NotNull(user);
        Assert.True(await users.CheckPasswordAsync(user, "Student123"));
        Assert.NotEqual("Student123", user.PasswordHash);
        Assert.Equal(user.Id, (await db.Members.SingleAsync()).UserId);
        Assert.Equal(new[] { AppRoles.Member }, await users.GetRolesAsync(user));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.IssueAsync(1, "OTHER", "Student123"));
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Theory]
    [InlineData(DomainStatus.Pending)]
    [InlineData(DomainStatus.Rejected)]
    public async Task UnapprovedApplicationCannotReceiveAccount(string status)
    {
        (await db.MembershipApplications.SingleAsync()).Status = status;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.IssueAsync(1, "SVTEST", "Student123"));
        Assert.Empty(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task WeakPasswordDoesNotCreateAccountOrLink()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.IssueAsync(1, "SVTEST", "123"));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Users.ToListAsync());
        Assert.Null((await db.Members.SingleAsync()).UserId);
    }

    [Fact]
    public async Task RoleFailureRollsBackCreatedUser()
    {
        db.Roles.RemoveRange(db.Roles);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.IssueAsync(1, "SVTEST", "Student123"));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Users.ToListAsync());
        Assert.Null((await db.Members.SingleAsync()).UserId);
    }

    [Fact]
    public async Task DuplicateUsernameDoesNotLinkAnExistingAccount()
    {
        Assert.True((await users.CreateAsync(new ApplicationUser { UserName = "SVTEST", Email = "other@example.test" }, "Student123")).Succeeded);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.IssueAsync(1, "SVTEST", "Student123"));
        db.ChangeTracker.Clear();
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Null((await db.Members.SingleAsync()).UserId);
    }

    [Fact]
    public void RegistrationRequiresDetailsAndRejectsFutureBirthYear()
    {
        var model = new MembershipApplicationViewModel { ClubId = 1, StudentCode = "SV", FullName = "Student", Email = "s@example.test" };
        var errors = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(model, new ValidationContext(model), errors, true));
        Assert.Contains(errors, x => x.MemberNames.Contains(nameof(model.Interests)));
        Assert.Contains(errors, x => x.MemberNames.Contains(nameof(model.JoinReason)));
        Assert.Contains(errors, x => x.MemberNames.Contains(nameof(model.Expectations)));
        Assert.Contains(errors, x => x.MemberNames.Contains(nameof(model.BirthYear)));
        model.Interests = "Music"; model.JoinReason = "Learn"; model.Expectations = "Friends"; model.BirthYear = DateTime.Today.Year + 1;
        Assert.Contains(model.Validate(new ValidationContext(model)), x => x.MemberNames.Contains(nameof(model.BirthYear)));
    }

    [Fact]
    public async Task ClubLeaderCanAccessOnlyMembersOfTheirClub()
    {
        await service.IssueAsync(1, "SVTEST", "Student123");
        var user = (await users.FindByNameAsync("SVTEST"))!;
        db.Clubs.Add(new Club { Id = 2, ClubName = "Other" });
        var peer = new Member { ClubId = 1, StudentCode = "PEER", FullName = "Peer", Email = "peer@example.test" };
        var outsider = new Member { ClubId = 2, StudentCode = "OTHER", FullName = "Other", Email = "other@example.test" };
        db.Members.AddRange(peer, outsider);
        await db.SaveChangesAsync();
        var principal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, AppRoles.ClubLeader)
        }, "Test"));
        var scope = new StudentClubManagement.Authorization.AccessScopeService(users, db);
        Assert.True(await scope.CanAccessMemberAsync(principal, peer.Id));
        Assert.False(await scope.CanAccessMemberAsync(principal, outsider.Id));
    }

    [Fact]
    public async Task LegacyLeaderRoleIsRenamedWithoutLosingAssignments()
    {
        var roles = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await roles.CreateAsync(new IdentityRole<int>("DEPARTMENT_LEADER"));
        var user = new ApplicationUser { UserName = "oldleader", Email = "leader@example.test" };
        await users.CreateAsync(user, "Student123");
        await users.AddToRoleAsync(user, "DEPARTMENT_LEADER");
        await DemoSeed.SeedAsync(provider);
        Assert.True(await users.IsInRoleAsync(user, AppRoles.ClubLeader));
        Assert.False(await roles.RoleExistsAsync("DEPARTMENT_LEADER"));
    }

    public void Dispose() { provider.Dispose(); connection.Dispose(); }
}

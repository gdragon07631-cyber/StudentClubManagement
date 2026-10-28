using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentClubManagement.Api;
using StudentClubManagement.Authorization;
using StudentClubManagement.Data;
using StudentClubManagement.Models;
using StudentClubManagement.Services;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Missing DefaultConnection.");
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connection, sqlServer =>
{
    sqlServer.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
}));
builder.Services.AddIdentityCore<ApplicationUser>(o =>
{
    o.Password.RequiredLength = 8; o.Password.RequireDigit = true; o.Password.RequireUppercase = true;
    o.Password.RequireNonAlphanumeric = false; o.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole<int>>().AddEntityFrameworkStores<ApplicationDbContext>().AddSignInManager().AddDefaultTokenProviders();

// ---- JWT: Frontend đăng nhập qua /api/auth/login, nhận token rồi gửi kèm Authorization: Bearer ----
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32) throw new InvalidOperationException("Jwt:Key phải có tối thiểu 32 ký tự (đặt qua biến môi trường Jwt__Key).");
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.MapInboundClaims = false; // giữ nguyên ClaimTypes.* như lúc phát hành token
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true, ValidateLifetime = true,
        NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role, ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { HideSensitiveUserFields } };
});
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<AccountProvisioningService>();
builder.Services.AddScoped<IAccessScopeService, AccessScopeService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient("AI", c => c.Timeout = TimeSpan.FromSeconds(20));

var app = builder.Build();
app.UseExceptionHandler(e => e.Run(async ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await ctx.Response.WriteAsJsonAsync(new ApiError("Lỗi hệ thống. Vui lòng thử lại sau."));
}));
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { name = "StudentClubManagement API", status = "running" })).AllowAnonymous();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DemoSeed.SeedAsync(scope.ServiceProvider);
}
app.Run();

// Không bao giờ trả hash mật khẩu / security stamp của ApplicationUser ra JSON.
static void HideSensitiveUserFields(JsonTypeInfo info)
{
    if (info.Type != typeof(ApplicationUser)) return;
    string[] hidden = [nameof(ApplicationUser.PasswordHash), nameof(ApplicationUser.SecurityStamp), nameof(ApplicationUser.ConcurrencyStamp),
        nameof(ApplicationUser.NormalizedUserName), nameof(ApplicationUser.NormalizedEmail), nameof(ApplicationUser.PhoneNumber),
        nameof(ApplicationUser.AccessFailedCount), nameof(ApplicationUser.LockoutEnd), nameof(ApplicationUser.LockoutEnabled), nameof(ApplicationUser.TwoFactorEnabled)];
    foreach (var p in info.Properties.Where(p => hidden.Contains(p.Name, StringComparer.OrdinalIgnoreCase)).ToList()) info.Properties.Remove(p);
}

public partial class Program;

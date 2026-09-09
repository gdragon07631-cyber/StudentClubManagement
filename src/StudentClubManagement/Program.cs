using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(o =>
{
    o.Password.RequiredLength = 8; o.Password.RequireDigit = true; o.Password.RequireUppercase = true;
    o.Password.RequireNonAlphanumeric = false; o.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/AccessDenied"; o.ExpireTimeSpan = TimeSpan.FromHours(8); });
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<AccountProvisioningService>();
builder.Services.AddScoped<IAccessScopeService, AccessScopeService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient("AI", c => c.Timeout = TimeSpan.FromSeconds(20));

var app = builder.Build();
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}").WithStaticAssets();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DemoSeed.SeedAsync(scope.ServiceProvider);
}
app.Run();
public partial class Program;

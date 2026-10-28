using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StudentClubManagement.Services;

var builder = WebApplication.CreateBuilder(args);
var apiBase = builder.Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Missing Api:BaseUrl.");

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ApiClient>(c => { c.BaseAddress = new Uri(apiBase.TrimEnd('/') + "/"); c.Timeout = TimeSpan.FromSeconds(30); });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromHours(8); o.SlidingExpiration = false;
});
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.MapStaticAssets();

// Chuyển lỗi 401/403/404 từ Backend thành hành vi tương ứng của website.
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (ApiStatusException ex) when (!ctx.Response.HasStarted)
    {
        ctx.Response.Clear();
        switch (ex.Status)
        {
            case HttpStatusCode.Unauthorized:
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ctx.Response.Redirect("/Account/Login"); break;
            case HttpStatusCode.Forbidden: ctx.Response.Redirect("/Account/AccessDenied"); break;
            default: ctx.Response.StatusCode = StatusCodes.Status404NotFound; break;
        }
    }
});
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}").WithStaticAssets();
app.Run();


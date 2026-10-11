using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;
using Newsletter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        // 요청 본문을 읽지 못하면(JSON 형식 오류 등) api.md 공통 실패 형식으로 응답한다
        options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(
            new ApiErrorResponse(new ApiErrorBody(ErrorCodes.InvalidInput, "입력값이 올바르지 않습니다.")));
    });

builder.Services.AddDbContext<NewsletterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Newsletter")));

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
});

// 쿠키 인증 (D-060): HttpOnly, SameSite=Strict, 만료 60분(요청이 있으면 연장)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Newsletter.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";

        // API 요청은 로그인 화면으로 보내지 않고 api.md 공통 실패 형식으로 응답한다
        options.Events.OnRedirectToLogin = context => WriteRedirectOrApiErrorAsync(
            context, StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, "로그인이 필요합니다.", context.RedirectUri);

        // 권한이 없는 화면에 들어오면 권한별 첫 화면으로 보낸다
        options.Events.OnRedirectToAccessDenied = context => WriteRedirectOrApiErrorAsync(
            context, StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, "권한이 없습니다.",
            HomePathResolver.GetHomePath(context.HttpContext.User.FindFirstValue(ClaimTypes.Role)));
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.AdminOnly, policy => policy.RequireRole(UserRole.Admin));
});

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection(MailOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IMailSender, GmailMailSender>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRecipientService, RecipientService>();

var app = builder.Build();

// API 요청에서 처리하지 못한 예외는 스택 트레이스 없이 api.md 공통 실패 형식으로 응답한다
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), apiApp =>
{
    apiApp.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        IExceptionHandlerFeature? exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        ILogger logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ApiException");
        logger.LogError(exceptionFeature?.Error, "API 처리 중 오류");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(new ApiErrorBody(ErrorCodes.InternalError, "오류가 발생했습니다. 잠시 후 다시 시도해 주세요.")));
    }));
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static Task WriteRedirectOrApiErrorAsync(RedirectContext<CookieAuthenticationOptions> context, int statusCode, string errorCode, string message, string redirectUri)
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ApiErrorResponse(new ApiErrorBody(errorCode, message)));
    }

    context.Response.Redirect(redirectUri);
    return Task.CompletedTask;
}

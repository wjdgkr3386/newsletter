using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newsletter.Models.Dtos;
using Newsletter.Services;

namespace Newsletter.Filters;

/// <summary>
/// POST 요청의 RequestVerificationToken 헤더를 검사한다 (D-060).
/// 실패하면 기본 동작(빈 400 응답) 대신 api.md 공통 실패 형식으로 응답한다
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiAntiforgeryAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            return;
        }

        IAntiforgery antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
        bool isValid = await antiforgery.IsRequestValidAsync(context.HttpContext);
        if (!isValid)
        {
            context.Result = new ObjectResult(new ApiErrorResponse(new ApiErrorBody(ErrorCodes.InvalidInput, "요청이 올바르지 않습니다. 화면을 새로고침한 뒤 다시 시도해 주세요.")))
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }
    }
}

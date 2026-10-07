using Microsoft.AspNetCore.Mvc;
using Newsletter.Filters;
using Newsletter.Models.Dtos;
using Newsletter.Services;

namespace Newsletter.Controllers.Api;

/// <summary>
/// JSON API 컨트롤러 공통. 응답을 api.md 공통 형식으로 만들고, POST 요청의 안티포저리 토큰을 검사한다 (D-060)
/// </summary>
[ApiController]
[ApiAntiforgery]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiSuccessResponse<T>(result.Data!));
        }

        ServiceError error = result.Error!;
        return StatusCode(error.StatusCode, new ApiErrorResponse(new ApiErrorBody(error.Code, error.Message)));
    }
}

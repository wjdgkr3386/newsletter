using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Services;

namespace Newsletter.Controllers;

/// <summary>
/// 관리자 화면 (D-017: 관리자만)
/// </summary>
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Send()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Recipients()
    {
        return View();
    }
}

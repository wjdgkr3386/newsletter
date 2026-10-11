using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Services;

namespace Newsletter.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        // 이미 로그인했으면 권한별 첫 화면으로 이동한다 (D-072)
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(HomePathResolver.GetHomePath(User.FindFirstValue(ClaimTypes.Role)));
        }

        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(HomePathResolver.GetHomePath(User.FindFirstValue(ClaimTypes.Role)));
        }

        return View();
    }
}

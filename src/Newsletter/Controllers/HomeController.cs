using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Models;
using Newsletter.Services;

namespace Newsletter.Controllers;

public class HomeController : Controller
{
    /// <summary>
    /// 루트 주소(/). 로그인하지 않았으면 로그인 화면, 로그인했으면 권한별 첫 화면으로 이동한다 (D-071)
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Redirect("/Account/Login");
        }

        return Redirect(HomePathResolver.GetHomePath(User.FindFirstValue(ClaimTypes.Role)));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

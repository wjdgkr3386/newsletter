using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Newsletter.Controllers;

/// <summary>
/// 히스토리 화면 (일반 사용자, 관리자)
/// </summary>
[Authorize]
public class HistoryController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}

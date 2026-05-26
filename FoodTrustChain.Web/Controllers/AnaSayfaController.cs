using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FoodTrustChain.Web.Controllers
{
    [AllowAnonymous]
    public class AnaSayfaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Hakkinda()
        {
            return View();
        }
    }
}
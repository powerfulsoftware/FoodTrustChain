using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrustChain.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public IActionResult PageNotFound()
        {
            ViewData["StatusCode"] = 404;
            ViewData["Title"] = "Sayfa Bulunamadı";
            ViewData["Message"] = "Aradığınız sayfa bulunamadı.";
            return View("Index");
        }

        public IActionResult PageUnauthorized()
        {
            ViewData["StatusCode"] = 403;
            ViewData["Title"] = "Yetkisiz Erişim";
            ViewData["Message"] = "Bu sayfaya erişim yetkiniz yok!";
            return View("Index");
        }
    }
}
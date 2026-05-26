using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodTrustChain.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FoodTrustChain.Web.Areas.Yonetici.Controllers
{
    [Area("Yonetici")]
    [Authorize(Roles = "Yonetici")]
    [Route("Yonetici/[controller]/[action]")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ToplamUye = await _context.Uyeler.CountAsync();
            ViewBag.ToplamUrun = await _context.Urunler.CountAsync();
            ViewBag.ToplamUretim = await _context.Uretimler.CountAsync();
            ViewBag.ToplamNakliye = await _context.Nakliyeler.CountAsync();
            ViewBag.ToplamDepolama = await _context.DepolamaIslemleri.CountAsync();
            ViewBag.ToplamTasit = await _context.Tasitler.CountAsync();
            ViewBag.ToplamBlokKayit = await _context.BlokZincirKayitlari.CountAsync();

            ViewBag.AktifUye = await _context.Uyeler.CountAsync(u => u.AktifMi);
            ViewBag.AktifUrun = await _context.Urunler.CountAsync(u => u.Durum != "Satildi");

            var sonUrunler = await _context.Urunler
                .Include(u => u.Uretici)
                .OrderByDescending(u => u.OlusturmaTarihi)
                .Take(5)
                .ToListAsync();

            var sonBlokKayitlar = await _context.BlokZincirKayitlari
                .OrderByDescending(b => b.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.SonUrunler = sonUrunler;
            ViewBag.SonBlokKayitlar = sonBlokKayitlar;

            return View();
        }
    }
}
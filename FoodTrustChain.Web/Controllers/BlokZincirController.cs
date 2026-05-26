using FoodTrustChain.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FoodTrustChain.Web.Controllers
{
    [Authorize(Roles = "Yonetici,Uretici,Depo,Lojistik,Market,Tuketici")]
    public class BlokZincirController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlokZincirController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var kayitlar = await _context.BlokZincirKayitlari
                .OrderByDescending(b => b.Id)
                .Take(100)
                .ToListAsync();
            return View(kayitlar);
        }

        public async Task<IActionResult> Detay(int? id)
        {
            if (id == null) return NotFound();

            var kayit = await _context.BlokZincirKayitlari
                .FirstOrDefaultAsync(k => k.Id == id);

            return kayit == null ? NotFound() : View(kayit);
        }

        public async Task<IActionResult> ZincirDogrula()
        {
            var kayitlar = await _context.BlokZincirKayitlari
                .OrderBy(k => k.Id)
                .ToListAsync();

            ViewBag.Gecerli = true;
            ViewBag.Mesaj = "";

            if (kayitlar.Count == 0)
            {
                ViewBag.Gecerli = false;
                ViewBag.Mesaj = "Zincir bos";
                return View();
            }

            for (int i = 1; i < kayitlar.Count; i++)
            {
                if (kayitlar[i].OncekiHash != kayitlar[i - 1].MevcutHash)
                {
                    ViewBag.Gecerli = false;
                    ViewBag.Mesaj = $"Zincir kopugu tespit edildi! Kayit ID: {kayitlar[i].Id}";
                    return View();
                }
            }

            ViewBag.Mesaj = $"Zincir gecerli. Toplam {kayitlar.Count} kayit.";
            return View();
        }
    }
}
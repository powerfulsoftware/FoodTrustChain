using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodTrustChain.Web.Data;
using FoodTrustChain.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoodTrustChain.Web.Areas.Yonetici.Controllers
{
    [Area("Yonetici")]
    [Authorize(Roles = "Yonetici")]
    [Route("Yonetici/[controller]/[action]")]
    public class AyarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AyarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ayarlar = await _context.Ayar.OrderByDescending(a => a.OlusturmaTarihi).ToListAsync();
            return View(ayarlar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Ayar ayar)
        {
            if (string.IsNullOrWhiteSpace(ayar.Anahtar))
            {
                TempData["Hata"] = "Anahtar alanı boş bırakılamaz.";
                return RedirectToAction(nameof(Index));
            }

            var mevcut = await _context.Ayar.FirstOrDefaultAsync(a => a.Anahtar == ayar.Anahtar);
            if (mevcut != null)
            {
                TempData["Hata"] = "Bu anahtar zaten mevcut.";
                return RedirectToAction(nameof(Index));
            }

            ayar.AktifMi = true;
            ayar.OlusturmaTarihi = DateTime.Now;
            _context.Ayar.Add(ayar);
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Ayar başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var ayar = await _context.Ayar.FindAsync(id);
            if (ayar != null)
            {
                _context.Ayar.Remove(ayar);
                await _context.SaveChangesAsync();
                TempData["Basari"] = "Ayar silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, string deger, string aciklama)
        {
            var ayar = await _context.Ayar.FindAsync(id);
            if (ayar != null)
            {
                ayar.Deger = deger;
                ayar.Aciklama = aciklama;
                await _context.SaveChangesAsync();
                TempData["Basari"] = "Ayar güncellendi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
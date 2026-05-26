using FoodTrustChain.Web.Data;
using FoodTrustChain.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FoodTrustChain.Web.Controllers
{
    [Authorize(Roles = "Yonetici")]
    public class UyelerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UyelerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string HashSifre(string sifre)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sifre));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        public async Task<IActionResult> Index()
        {
            var uyeler = await _context.Uyeler
                .OrderByDescending(u => u.OlusturmaTarihi)
                .ToListAsync();
            return View(uyeler);
        }

        public IActionResult Ekle()
        {
            ViewBag.Roller = new SelectList(new[] { "Yonetici", "Uretici", "Depo", "Lojistik", "Market", "Tuketici" });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ekle(Uyeler uye)
        {
            if (ModelState.IsValid)
            {
                uye.Sifre = HashSifre(uye.Sifre);
                uye.AktifMi = true;
                uye.OlusturmaTarihi = DateTime.Now;
                _context.Uyeler.Add(uye);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roller = new SelectList(new[] { "Yonetici", "Uretici", "Depo", "Lojistik", "Market", "Tuketici" }, uye.Rol);
            return View(uye);
        }

        public async Task<IActionResult> Detay(int? id)
        {
            if (id == null) return NotFound();

            var uye = await _context.Uyeler.FindAsync(id);
            return uye == null ? NotFound() : View(uye);
        }

        public async Task<IActionResult> RolDegistir(int? id)
        {
            if (id == null) return NotFound();

            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();

            ViewBag.Roller = new SelectList(new[] { "Yonetici", "Uretici", "Depo", "Lojistik", "Market", "Tuketici" }, uye.Rol);
            return View(uye);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolDegistir(int id, string yeniRol)
        {
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();

            var gecerliRoller = new[] { "Yonetici", "Uretici", "Depo", "Lojistik", "Market", "Tuketici" };
            if (!gecerliRoller.Contains(yeniRol))
            {
                TempData["Hata"] = "Geçersiz rol seçildi.";
                return RedirectToAction(nameof(Index));
            }

            uye.Rol = yeniRol;
            await _context.SaveChangesAsync();

            TempData["Basari"] = $"{uye.Ad} {uye.Soyad} kullanıcısının rolü '{yeniRol}' olarak güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
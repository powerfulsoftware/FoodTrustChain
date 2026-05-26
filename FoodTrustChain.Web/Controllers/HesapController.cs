using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FoodTrustChain.Web.Data;
using FoodTrustChain.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FoodTrustChain.Web.Controllers
{
    public class HesapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HesapController(ApplicationDbContext context)
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

        [AllowAnonymous]
        public IActionResult Giris()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Giris(string Eposta, string Sifre)
        {
            if (string.IsNullOrEmpty(Eposta) || string.IsNullOrEmpty(Sifre))
            {
                ViewBag.Hata = "E-posta ve sifre gereklidir.";
                return View();
            }

            var hashedSifre = HashSifre(Sifre);
            var uye = await _context.Uyeler
                .FirstOrDefaultAsync(u => u.Eposta == Eposta && u.Sifre == hashedSifre && u.AktifMi);

            if (uye == null)
            {
                ViewBag.Hata = "Gecersiz eposta veya sifre.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, uye.Id.ToString()),
                new Claim(ClaimTypes.Name, uye.Ad + " " + uye.Soyad),
                new Claim(ClaimTypes.Email, uye.Eposta),
                new Claim(ClaimTypes.Role, uye.Rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "AnaSayfa");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Giris", "Hesap");
        }

        [AllowAnonymous]
        public IActionResult Kayit()
        {
            ViewBag.Roller = new List<string> { "Uretici", "Depo", "Lojistik", "Market" };
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Kayit(Uyeler uye, string SifreTekrar)
        {
            if (uye.Sifre != SifreTekrar)
            {
                ViewBag.Hata = "Sifreler eslesmiyor.";
                ViewBag.Roller = new List<string> { "Uretici", "Depo", "Lojistik", "Market" };
                return View(uye);
            }

            if (await _context.Uyeler.AnyAsync(u => u.Eposta == uye.Eposta))
            {
                ViewBag.Hata = "Bu e-posta zaten kullaniliyor.";
                ViewBag.Roller = new List<string> { "Uretici", "Depo", "Lojistik", "Market" };
                return View(uye);
            }

            uye.Sifre = HashSifre(uye.Sifre);
            uye.AktifMi = true;
            uye.OlusturmaTarihi = DateTime.Now;
            uye.Rol = "Tuketici";

            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Giris));
        }
    }
}
using FoodTrustChain.Web.Data;
using FoodTrustChain.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FoodTrustChain.Web.Controllers
{
    [Authorize(Roles = "Yonetici,Uretici")]
    public class UretimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UretimController(ApplicationDbContext context)
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

        [Authorize(Roles = "Yonetici,Uretici")]
        public async Task<IActionResult> Index()
        {
            var uretimler = await _context.Uretimler
                .Include(u => u.Urun)
                .Include(u => u.Uretici)
                .OrderByDescending(u => u.OlusturmaTarihi)
                .ToListAsync();
            return View(uretimler);
        }

        [Authorize(Roles = "Yonetici,Uretici")]
        public IActionResult Ekle()
        {
            var urunler = _context.Urunler.Select(u => new { u.Id, u.Ad }).ToList();
            var ureticiler = _context.Uyeler.Where(u => u.Rol == "Uretici")
                .Select(u => new { u.Id, TamAd = u.Ad + " " + u.Soyad }).ToList();
            ViewBag.Urunler = new SelectList(urunler, "Id", "Ad");
            ViewBag.Ureticiler = new SelectList(ureticiler, "Id", "TamAd");
            ViewBag.Birimler = new SelectList(new[] { "kg", "ton", "adet", "litre", "koli" });
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Yonetici,Uretici")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle([Bind("UrunId,UreticiId,CiftlikAdi,Konum,HasatTarihi,Miktar,Birim")] Uretim uretim)
        {
            var urunler = _context.Urunler.Select(u => new { u.Id, u.Ad }).ToList();
            var ureticiler = _context.Uyeler.Where(u => u.Rol == "Uretici")
                .Select(u => new { u.Id, TamAd = u.Ad + " " + u.Soyad }).ToList();
            ViewBag.Urunler = new SelectList(urunler, "Id", "Ad");
            ViewBag.Ureticiler = new SelectList(ureticiler, "Id", "TamAd");
            ViewBag.Birimler = new SelectList(new[] { "kg", "ton", "adet", "litre", "koli" });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                Console.WriteLine($"ModelState hatalı: {errors}");
                return View(uretim);
            }

            if (uretim.UrunId == 0 || uretim.UreticiId == 0)
            {
                Console.WriteLine("Ürün veya Üretici seçilmedi");
                ModelState.AddModelError("", "Lütfen ürün ve üretici seçin");
                return View(uretim);
            }

            Console.WriteLine($"Ürün ID: {uretim.UrunId}, Üretici ID: {uretim.UreticiId}, Çiftlik: {uretim.CiftlikAdi}, Miktar: {uretim.Miktar}");

            uretim.OlusturmaTarihi = DateTime.Now;
                uretim.GorselYolu = "/images/default.png";

            var sonKayit = await _context.BlokZincirKayitlari
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            var hashIcerik = $"{uretim.UrunId}|{uretim.CiftlikAdi}|{uretim.HasatTarihi}|{uretim.Miktar}|{DateTime.Now.Ticks}";
            uretim.HashDegeri = HashSifre(hashIcerik);

            var yeniKayit = new BlokZincirKayit
            {
                OncekiHash = sonKayit?.MevcutHash ?? "0000000000000000000000000000000000000000000000000000000000000000",
                IslemTipi = "Uretim",
                IslemIcerigi = $"Uretim kaydi: {uretim.CiftlikAdi}, Urun ID: {uretim.UrunId}, Miktar: {uretim.Miktar} {uretim.Birim}",
                ZamanDamgasi = DateTime.Now,
                Nonce = 0
            };
            yeniKayit.MevcutHash = HashSifre(yeniKayit.OncekiHash + yeniKayit.IslemIcerigi + yeniKayit.ZamanDamgasi.Ticks + yeniKayit.Nonce);

            _context.BlokZincirKayitlari.Add(yeniKayit);
            await _context.SaveChangesAsync();

            uretim.BlokZincirKayitId = yeniKayit.Id;
            _context.Uretimler.Add(uretim);
            await _context.SaveChangesAsync();

            var urun = await _context.Urunler.FindAsync(uretim.UrunId);
            if (urun != null)
            {
                urun.Durum = "Uretimde";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Yonetici,Uretici")]
        public async Task<IActionResult> Detay(int? id)
        {
            if (id == null) return NotFound();

            var uretim = await _context.Uretimler
                .Include(u => u.Urun)
                .Include(u => u.Uretici)
                .Include(u => u.BlokZincirKayit)
                .FirstOrDefaultAsync(u => u.Id == id);

            return uretim == null ? NotFound() : View(uretim);
        }
    }
}
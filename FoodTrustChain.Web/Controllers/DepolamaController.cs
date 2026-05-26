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
    [Authorize(Roles = "Yonetici,Depo,Lojistik")]
    public class DepolamaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepolamaController(ApplicationDbContext context)
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
            if (!User.IsInRole("Yonetici") && !User.IsInRole("Depo") && !User.IsInRole("Lojistik"))
                return Forbid();

            var depolar = await _context.DepolamaIslemleri
                .Include(d => d.Urun)
                .OrderByDescending(d => d.OlusturmaTarihi)
                .ToListAsync();
            return View(depolar);
        }

        [Authorize(Roles = "Yonetici,Depo,Lojistik")]
        public IActionResult Ekle()
        {
            ViewBag.Urunler = new SelectList(_context.Urunler.Where(u => u.Durum != "Satildi"), "Id", "Ad");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Yonetici,Depo,Lojistik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle([Bind("UrunId,DepoAdi,DepoAdresi,Sicaklik,NemOrani,GirisTarihi")] Depolama depolama)
        {
            var urunler = _context.Urunler.Where(u => u.Durum != "Satildi").Select(u => new { u.Id, u.Ad }).ToList();
            ViewBag.Urunler = new SelectList(urunler, "Id", "Ad");

            ModelState.Remove("Urun");
            ModelState.Remove("HashDegeri");
            ModelState.Remove("OlusturmaTarihi");
            ModelState.Remove("BlokZincirKayitId");
            ModelState.Remove("CikisTarihi");
            ModelState.Remove("Urun");

            if (depolama.UrunId == 0)
            {
                ModelState.AddModelError("", "Lütfen ürün seçin");
                return View(depolama);
            }

            if (ModelState.IsValid)
            {
                depolama.OlusturmaTarihi = DateTime.Now;
                if (depolama.GirisTarihi == null)
                    depolama.GirisTarihi = DateTime.Now;

                var sonKayit = await _context.BlokZincirKayitlari
                    .OrderByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                var hashIcerik = $"{depolama.UrunId}|{depolama.DepoAdi}|{depolama.GirisTarihi}|{depolama.Sicaklik}|{DateTime.Now.Ticks}";
                depolama.HashDegeri = HashSifre(hashIcerik);

                var yeniKayit = new BlokZincirKayit
                {
                    OncekiHash = sonKayit?.MevcutHash ?? "0000000000000000000000000000000000000000000000000000000000000000",
                    IslemTipi = "Depolama",
                    IslemIcerigi = $"Depolama kaydi: {depolama.DepoAdi}, Urun ID: {depolama.UrunId}, Sicaklik: {depolama.Sicaklik}C",
                    ZamanDamgasi = DateTime.Now,
                    Nonce = 0
                };
                yeniKayit.MevcutHash = HashSifre(yeniKayit.OncekiHash + yeniKayit.IslemIcerigi + yeniKayit.ZamanDamgasi.Ticks + yeniKayit.Nonce);

                _context.BlokZincirKayitlari.Add(yeniKayit);
                await _context.SaveChangesAsync();

                depolama.BlokZincirKayitId = yeniKayit.Id;
                _context.DepolamaIslemleri.Add(depolama);
                await _context.SaveChangesAsync();

                var urun = await _context.Urunler.FindAsync(depolama.UrunId);
                if (urun != null)
                {
                    urun.Durum = "Depoda";
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(depolama);
        }

        public async Task<IActionResult> Cikis(int id)
        {
            var depolama = await _context.DepolamaIslemleri.FindAsync(id);
            if (depolama != null)
            {
                depolama.CikisTarihi = DateTime.Now;
                await _context.SaveChangesAsync();

                var urun = await _context.Urunler.FindAsync(depolama.UrunId);
                if (urun != null)
                {
                    urun.Durum = "Tasinyor";
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
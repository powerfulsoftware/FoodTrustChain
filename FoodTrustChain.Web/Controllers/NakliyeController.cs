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
    [Authorize(Roles = "Yonetici,Lojistik")]
    public class NakliyeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NakliyeController(ApplicationDbContext context)
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
            if (!User.IsInRole("Yonetici") && !User.IsInRole("Lojistik"))
                return Forbid();

            var nakliyeler = await _context.Nakliyeler
                .Include(n => n.Urun)
                .Include(n => n.Tasit)
                .OrderByDescending(n => n.OlusturmaTarihi)
                .ToListAsync();
            return View(nakliyeler);
        }

        [Authorize(Roles = "Yonetici,Lojistik")]
        public IActionResult Ekle()
        {
            ViewBag.Urunler = new SelectList(_context.Urunler.Where(u => u.Durum == "Depoda" || u.Durum == "Tasinyor"), "Id", "Ad");
            ViewBag.Tasitler = new SelectList(_context.Tasitler.Where(t => t.AktifMi), "Id", "PlakaNo");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Yonetici,Lojistik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Nakliye nakliye)
        {
            ModelState.Remove("HashDegeri");
            ModelState.Remove("OlusturmaTarihi");
            ModelState.Remove("BlokZincirKayitId");
            ModelState.Remove("TasimaBitisTarihi");
            ModelState.Remove("SoforAdi");
            ModelState.Remove("Urun");
            ModelState.Remove("Tasit");

            if (ModelState.IsValid)
            {
                nakliye.OlusturmaTarihi = DateTime.Now;
                nakliye.TasimaBaslangicTarihi = DateTime.Now;

                var tasit = await _context.Tasitler.FindAsync(nakliye.TasitId);
                if (tasit != null)
                {
                    nakliye.SoforAdi = tasit.SoforAdi;
                }

                var sonKayit = await _context.BlokZincirKayitlari
                    .OrderByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                var hashIcerik = $"{nakliye.UrunId}|{nakliye.TasitId}|{nakliye.BaslangicAdresi}|{nakliye.BitisAdresi}|{DateTime.Now.Ticks}";
                nakliye.HashDegeri = HashSifre(hashIcerik);

                var yeniKayit = new BlokZincirKayit
                {
                    OncekiHash = sonKayit?.MevcutHash ?? "0000000000000000000000000000000000000000000000000000000000000000",
                    IslemTipi = "Nakliye",
                    IslemIcerigi = $"Nakliye kaydi: {nakliye.BaslangicAdresi} -> {nakliye.BitisAdresi}, Urun ID: {nakliye.UrunId}",
                    ZamanDamgasi = DateTime.Now,
                    Nonce = 0
                };
                yeniKayit.MevcutHash = HashSifre(yeniKayit.OncekiHash + yeniKayit.IslemIcerigi + yeniKayit.ZamanDamgasi.Ticks + yeniKayit.Nonce);

                _context.BlokZincirKayitlari.Add(yeniKayit);
                await _context.SaveChangesAsync();

                nakliye.BlokZincirKayitId = yeniKayit.Id;
                _context.Nakliyeler.Add(nakliye);
                await _context.SaveChangesAsync();

                var urun = await _context.Urunler.FindAsync(nakliye.UrunId);
                if (urun != null)
                {
                    urun.Durum = "Tasinyor";
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Urunler = new SelectList(_context.Urunler, "Id", "Ad", nakliye.UrunId);
            ViewBag.Tasitler = new SelectList(_context.Tasitler.Where(t => t.AktifMi), "Id", "PlakaNo", nakliye.TasitId);
            return View(nakliye);
        }

        public async Task<IActionResult> Teslim(int id)
        {
            var nakliye = await _context.Nakliyeler.FindAsync(id);
            if (nakliye != null)
            {
                nakliye.TasimaBitisTarihi = DateTime.Now;
                await _context.SaveChangesAsync();

                var urun = await _context.Urunler.FindAsync(nakliye.UrunId);
                if (urun != null)
                {
                    urun.Durum = "SatisNoktasinda";
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
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
    [Authorize(Roles = "Yonetici,Uretici,Depo,Lojistik,Market")]
    public class UrunController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UrunController(ApplicationDbContext context)
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
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Yonetici") && !User.IsInRole("Uretici") && !User.IsInRole("Depo") && !User.IsInRole("Lojistik") && !User.IsInRole("Market"))
                return Forbid();

            var urunler = await _context.Urunler
                .Include(u => u.Uretici)
                .OrderByDescending(u => u.OlusturmaTarihi)
                .ToListAsync();
            return View(urunler);
        }

        [Authorize(Roles = "Yonetici,Uretici,Depo,Lojistik,Market")]
        public async Task<IActionResult> Detay(int? id)
        {
            if (id == null)
                return NotFound();

            var urun = await _context.Urunler
                .Include(u => u.Uretici)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (urun == null)
                return NotFound();

            var uretimler = await _context.Uretimler
                .Where(u => u.UrunId == urun.Id)
                .OrderBy(u => u.OlusturmaTarihi)
                .ToListAsync();

            var depolamaIslemleri = await _context.DepolamaIslemleri
                .Where(d => d.UrunId == urun.Id)
                .OrderBy(d => d.OlusturmaTarihi)
                .ToListAsync();

            var nakliyeler = await _context.Nakliyeler
                .Include(n => n.Tasit)
                .Where(n => n.UrunId == urun.Id)
                .OrderBy(n => n.OlusturmaTarihi)
                .ToListAsync();

            ViewBag.Uretimler = uretimler;
            ViewBag.DepolamaIslemleri = depolamaIslemleri;
            ViewBag.Nakliyeler = nakliyeler;

            return View(urun);
        }

        public IActionResult Ekle()
        {
            ViewBag.Ureticiler = new SelectList(_context.Uyeler.Where(u => u.Rol == "Uretici"), "Id", "Ad");
            ViewBag.Kategoriler = new SelectList(new[] { "Et", "Sut", "Sebze", "Meyve", "Tahil", "Yumurta" });
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Yonetici,Uretici,Depo,Lojistik,Market")]
        public async Task<IActionResult> Ekle(Urun urun)
        {
            if (string.IsNullOrEmpty(urun.Ad) || urun.UreticiId == 0)
            {
                ViewBag.Hata = "Ürün adı ve üretici zorunludur.";
                ViewBag.Ureticiler = new SelectList(_context.Uyeler.Where(u => u.Rol == "Uretici"), "Id", "Ad");
                ViewBag.Kategoriler = new SelectList(new[] { "Et", "Sut", "Sebze", "Meyve", "Tahil", "Yumurta" });
                return View(urun);
            }

            urun.Guid = Guid.NewGuid();
            urun.OlusturmaTarihi = DateTime.Now;
            urun.Durum = "Uretimde";

            _context.Urunler.Add(urun);
            await _context.SaveChangesAsync();

            var sonKayit = await _context.BlokZincirKayitlari
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();

            var yeniKayit = new BlokZincirKayit
            {
                OncekiHash = sonKayit?.MevcutHash ?? "0000000000000000000000000000000000000000000000000000000000000000",
                IslemTipi = "UrunEkleme",
                IslemIcerigi = $"Yeni urun eklendi: {urun.Ad} (ID: {urun.Id})",
                ZamanDamgasi = DateTime.Now,
                Nonce = 0
            };
            yeniKayit.MevcutHash = HashSifre(yeniKayit.OncekiHash + yeniKayit.IslemIcerigi + yeniKayit.ZamanDamgasi.Ticks + yeniKayit.Nonce);
            _context.BlokZincirKayitlari.Add(yeniKayit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Duzenle(int? id)
        {
            if (id == null)
                return NotFound();

            var urun = await _context.Urunler.FindAsync(id);
            if (urun == null)
                return NotFound();

            ViewBag.Ureticiler = new SelectList(_context.Uyeler.Where(u => u.Rol == "Uretici"), "Id", "Ad", urun.UreticiId);
            ViewBag.Kategoriler = new SelectList(new[] { "Et", "Sut", "Sebze", "Meyve", "Tahil", "Yumurta" });
            ViewBag.Durumlar = new SelectList(new[] { "Uretimde", "Depoda", "Tasinyor", "SatisNoktasinda", "Satildi" });
            return View(urun);
        }

        [HttpPost]
        public async Task<IActionResult> Duzenle(Urun urun)
        {
            if (ModelState.IsValid)
            {
                _context.Update(urun);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Ureticiler = new SelectList(_context.Uyeler.Where(u => u.Rol == "Uretici"), "Id", "Ad", urun.UreticiId);
            return View(urun);
        }

        public async Task<IActionResult> Sil(int? id)
        {
            if (id == null)
                return NotFound();

            var urun = await _context.Urunler.FindAsync(id);
            if (urun != null)
            {
                _context.Urunler.Remove(urun);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
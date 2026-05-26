using FoodTrustChain.Web.Data;
using FoodTrustChain.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoodTrustChain.Web.Controllers
{
    [Authorize(Roles = "Yonetici,Uretici,Depo,Lojistik,Market,Tuketici")]
    public class QRKodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QRKodController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Tara()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Tara(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                ViewBag.Hata = "Gecerli bir GUID giriniz.";
                return View();
            }

            Guid urunGuid;
            if (!Guid.TryParse(guid, out urunGuid))
            {
                ViewBag.Hata = "Gecerli bir GUID formati giriniz.";
                return View();
            }

            var urun = await _context.Urunler
                .Include(u => u.Uretici)
                .FirstOrDefaultAsync(u => u.Guid == urunGuid);

            if (urun == null)
            {
                ViewBag.Hata = "Bu GUID ile urun bulunamadi.";
                return View();
            }

            var uretimler = await _context.Uretimler
                .Where(r => r.UrunId == urun.Id)
                .OrderBy(r => r.OlusturmaTarihi)
                .ToListAsync();

            var depolamaIslemleri = await _context.DepolamaIslemleri
                .Where(d => d.UrunId == urun.Id)
                .OrderBy(d => d.OlusturmaTarihi)
                .ToListAsync();

            var nakliyeler = await _context.Nakliyeler
                .Where(n => n.UrunId == urun.Id)
                .OrderBy(n => n.OlusturmaTarihi)
                .ToListAsync();

            ViewBag.Urun = urun;
            ViewBag.Uretimler = uretimler;
            ViewBag.DepolamaIslemleri = depolamaIslemleri;
            ViewBag.Nakliyeler = nakliyeler;

            return View("Sonuc");
        }

        public IActionResult Olustur(int urunId)
        {
            var urun = _context.Urunler.Find(urunId);
            if (urun == null)
                return NotFound();

            var url = Url.Action("Tara", "QRKod", new { guid = urun.Guid }, Request.Scheme);

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] bytes = qrCode.GetGraphic(10);
                    return File(bytes, "image/png");
                }
            }
        }
    }
}
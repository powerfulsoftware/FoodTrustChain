using FoodTrustChain.Web.Data;
using FoodTrustChain.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FoodTrustChain.Web.Controllers
{
    [Authorize(Roles = "Yonetici,Lojistik")]
    public class TasitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasitController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Yonetici") && !User.IsInRole("Lojistik"))
                return Forbid();

            var tasitler = await _context.Tasitler
                .Include(t => t.Nakliyeler)
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();
            return View(tasitler);
        }

        [Authorize(Roles = "Yonetici,Lojistik")]
        public IActionResult Ekle()
        {
            ViewBag.AracTipler = new SelectList(new[] { "Kamyon", "Tir", "Minibus", "Kamyonet" });
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Yonetici,Lojistik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Tasit tasit)
        {
            ModelState.Remove("OlusturmaTarihi");
            ModelState.Remove("AktifMi");
            ModelState.Remove("Nakliyeler");

            if (ModelState.IsValid)
            {
                tasit.OlusturmaTarihi = System.DateTime.Now;
                tasit.AktifMi = true;
                _context.Tasitler.Add(tasit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AracTipler = new SelectList(new[] { "Kamyon", "Tir", "Minibus", "Kamyonet" });
            return View(tasit);
        }

        public async Task<IActionResult> Detay(int? id)
        {
            if (id == null) return NotFound();

            var tasit = await _context.Tasitler
                .Include(t => t.Nakliyeler)
                .FirstOrDefaultAsync(t => t.Id == id);

            return tasit == null ? NotFound() : View(tasit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var tasit = await _context.Tasitler
                .Include(t => t.Nakliyeler)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tasit == null)
                return NotFound();

            var aktifNakliye = tasit.Nakliyeler?.Any(n => n.TasimaBitisTarihi == null);
            if (aktifNakliye == true)
            {
                TempData["Hata"] = "Bu taşıtta aktif nakliye işlemi olduğu için silinemez.";
                return RedirectToAction(nameof(Index));
            }

            _context.Tasitler.Remove(tasit);
            await _context.SaveChangesAsync();
            TempData["Basari"] = "Taşıt başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
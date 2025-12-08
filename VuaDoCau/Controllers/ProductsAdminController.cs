using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    public class ProductsAdminController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsAdminController(VuaDoCauDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // =====================================
        // HÀM TẠO SLUG
        // =====================================
        private string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";

            name = name.Trim().ToLowerInvariant();

            var normalized = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            string slug = sb.ToString().Normalize(NormalizationForm.FormC);

            slug = slug.Replace("đ", "d")
                       .Replace(" ", "-")
                       .Replace("/", "-")
                       .Replace(".", "")
                       .Replace(",", "")
                       .Replace(":", "")
                       .Replace(";", "");

            return slug;
        }

        private async Task<string> GenerateUniqueSlugAsync(string name)
        {
            var baseSlug = GenerateSlug(name);
            var slug = baseSlug;
            int count = 1;

            while (await _db.Products.AnyAsync(p => p.Slug == slug))
            {
                slug = $"{baseSlug}-{count}";
                count++;
            }

            return slug;
        }

        // =====================================
        // LƯU ẢNH
        // =====================================
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string folder = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/" + fileName;
        }

        // =====================================
        // INDEX
        // =====================================
        public async Task<IActionResult> Index()
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        // =====================================
        // CREATE - GET
        // =====================================
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            return View();
        }

        // =====================================
        // CREATE - POST
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? ImageFile)
        {
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            // Tạo slug mới
            model.Slug = await GenerateUniqueSlugAsync(model.Name);

            // Lưu ảnh nếu có
            if (ImageFile != null && ImageFile.Length > 0)
                model.ImageUrl = await SaveImageAsync(ImageFile);
            else if (string.IsNullOrWhiteSpace(model.ImageUrl))
                model.ImageUrl = "/images/can2.jpg"; // ảnh mặc định

            // Số lượt mua khi tạo mới luôn là 0
            model.Purchased = 0;

            // 🌟 THÊM STOCK
            model.Stock = model.Stock; // (đã nhập trong form)

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã thêm sản phẩm mới!";
            return RedirectToAction(nameof(Index));
        }

        // =====================================
        // EDIT - GET
        // =====================================
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", p.CategoryId);
            return View(p);
        }

        // =====================================
        // EDIT - POST
        // =====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model, IFormFile? ImageFile)
        {
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            var p = await _db.Products.FindAsync(model.Id);
            if (p == null) return NotFound();

            // Nếu đổi tên ⇒ tạo slug mới
            if (p.Name != model.Name)
                p.Slug = await GenerateUniqueSlugAsync(model.Name);

            // Cập nhật thông tin
            p.Name = model.Name;
            p.Sku = model.Sku;
            p.Summary = model.Summary;
            p.Price = model.Price;
            p.OldPrice = model.OldPrice;
            p.CategoryId = model.CategoryId;

            // 🌟 CẬP NHẬT STOCK
            p.Stock = model.Stock;

            // ảnh
            if (ImageFile != null && ImageFile.Length > 0)
                p.ImageUrl = await SaveImageAsync(ImageFile);
            else if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                p.ImageUrl = model.ImageUrl;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =====================================
        // DELETE - GET
        // =====================================
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products
                .Include(c => c.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return NotFound();

            return View(p);
        }

        // =====================================
        // DELETE - POST
        // =====================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();

            _db.Products.Remove(p);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã xóa sản phẩm!";
            return RedirectToAction(nameof(Index));
        }
    }
}

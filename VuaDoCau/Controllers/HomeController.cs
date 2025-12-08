using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VuaDoCau.Data;
using VuaDoCau.Models;
using System.Collections.Generic;

namespace VuaDoCau.Controllers
{
    public class HomeController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        public HomeController(VuaDoCauDbContext db) => _db = db;

        public IActionResult Index()
        {
            // Lấy 4 danh mục có nhiều sản phẩm nhất + kèm luôn Products
            var cats = _db.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .OrderByDescending(c => c.Products.Count)
                .Take(4)
                .ToList();

            return View(cats); // Model: IEnumerable<Category>
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}

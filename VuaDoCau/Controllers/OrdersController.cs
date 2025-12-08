using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Text.Json;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    public class OrdersController : Controller
    {
        private const string CART_KEY = "CART";
        private const string CHECKOUT_SELECTION_KEY = "CHECKOUT_SELECTION";

        private readonly VuaDoCauDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;

        public OrdersController(VuaDoCauDbContext db, UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        public class CheckoutForm
        {
            [Required] public string ReceiverName { get; set; } = "";
            [Required] public string Phone { get; set; } = "";
            public string? Email { get; set; }
            [Required] public string Address { get; set; } = "";
            public string? Note { get; set; }
        }

        // ========== SESSION HELPERS ==========
        private List<Line> GetCartRaw()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            return string.IsNullOrEmpty(json)
                ? new List<Line>()
                : JsonSerializer.Deserialize<List<Line>>(json) ?? new List<Line>();
        }

        private List<int>? GetSelection()
        {
            var json = HttpContext.Session.GetString(CHECKOUT_SELECTION_KEY);
            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<List<int>>(json);
        }

        private class Line { public int ProductId { get; set; } public int Quantity { get; set; } }

        // ========== BUILD ITEMS ==========
        private List<dynamic> BuildItems()
        {
            var cart = GetCartRaw();
            var selection = GetSelection();

            if (selection != null && selection.Any())
                cart = cart.Where(c => selection.Contains(c.ProductId)).ToList();

            if (!cart.Any())
                return new List<dynamic>();

            var ids = cart.Select(c => c.ProductId).ToList();

            var prods = _db.Products.AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p);

            var list = new List<dynamic>();

            foreach (var c in cart)
            {
                if (!prods.TryGetValue(c.ProductId, out var p)) continue;

                dynamic it = new ExpandoObject();
                it.ProductId = p.Id;
                it.Name = p.Name;
                it.ImageUrl = string.IsNullOrWhiteSpace(p.ImageUrl) ? "/images/no-image.png" : p.ImageUrl;
                it.UnitPrice = p.Price;
                it.Quantity = c.Quantity;
                it.Subtotal = p.Price * c.Quantity;

                list.Add(it);
            }

            return list;
        }

        private void FillTotals()
        {
            var items = BuildItems();
            decimal subtotal = items.Sum(i => (decimal)i.Subtotal);

            ViewBag.Items = items;
            ViewBag.Subtotal = subtotal;
            ViewBag.Shipping = items.Any() ? 25000 : 0;
            ViewBag.Total = subtotal + (items.Any() ? 25000 : 0);
        }

        // ========== CHECKOUT GET ==========
        public IActionResult Checkout()
        {
            var items = BuildItems();
            if (!items.Any())
            {
                TempData["CartError"] = "Không có sản phẩm nào để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            FillTotals();

            var vm = new CheckoutForm();

            if (User.Identity?.IsAuthenticated == true)
            {
                var u = _userMgr.GetUserAsync(User).Result;
                if (u != null)
                {
                    vm.ReceiverName = u.FullName ?? "";
                    vm.Phone = u.PhoneNumber ?? "";
                    vm.Email = u.Email;
                    vm.Address = u.Address ?? "";
                }
            }

            return View(vm);
        }

        // ========== CHECKOUT POST ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutForm form)
        {
            var items = BuildItems();
            if (!items.Any())
            {
                TempData["CartError"] = "Không có sản phẩm nào để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                FillTotals();
                return View(form);
            }

            var user = await _userMgr.GetUserAsync(User);

            var order = new Order
            {
                UserId = user?.Id ?? "",
                User = user,
                CreatedAt = DateTime.UtcNow,
                ReceiverName = form.ReceiverName,
                Phone = form.Phone,
                Email = form.Email ?? "",
                Address = form.Address,
                Note = form.Note,
                Subtotal = items.Sum(i => (decimal)i.Subtotal),
                ShippingFee = 25000,
                Status = "Pending"
            };

            var ids = items.Select(i => (int)i.ProductId).ToList();
            var products = await _db.Products
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            // ================================
            // 🌟 KHÔNG TRỪ TỒN KHO TẠI ĐÂY
            // 🌟 CHỈ TĂNG PURCHASED
            // ================================
            foreach (dynamic it in items)
            {
                var qty = (int)it.Quantity;
                var id = (int)it.ProductId;

                order.Items.Add(new OrderItem
                {
                    ProductId = id,
                    Quantity = qty,
                    UnitPrice = (decimal)it.UnitPrice
                });

                if (qty > 0 && products.TryGetValue(id, out var p))
                {
                    p.Purchased += qty;

                    // ❌ XOÁ BỎ—KHÔNG TRỪ STOCK:
                    // p.Stock = Math.Max(0, p.Stock - qty);
                }
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Remove purchased items from cart
            var cart = GetCartRaw();
            cart = cart.Where(c => !ids.Contains(c.ProductId)).ToList();
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));

            // Clean selection
            HttpContext.Session.Remove(CHECKOUT_SELECTION_KEY);

            return RedirectToAction("Success", new { id = order.Id });
        }

        public async Task<IActionResult> Success(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? RedirectToAction("Index", "Home") : View(order);
        }
    }
}

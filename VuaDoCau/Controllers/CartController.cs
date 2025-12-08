using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VuaDoCau.Data;

namespace VuaDoCau.Controllers
{
    public class CartController : Controller
    {
        private const string CART_KEY = "CART";
        private const string CHECKOUT_SELECTION_KEY = "CHECKOUT_SELECTION";

        private readonly VuaDoCauDbContext _db;

        public CartController(VuaDoCauDbContext db)
        {
            _db = db;
        }

        // ========================== LOGIN CHECK ==========================
        private IActionResult? RequireLogin()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return Redirect($"/Account/Login?returnUrl={returnUrl}");
            }
            return null;
        }

        // ========================== MODELS ==========================
        private class Line
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class ItemVM
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = "";
            public string? ImageUrl { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal => UnitPrice * Quantity;
        }

        public class CartVM
        {
            public List<ItemVM> Items { get; set; } = new();
            public decimal Shipping { get; set; } = 25000;
            public decimal Subtotal => Items.Sum(i => i.Subtotal);
            public decimal Total => Items.Any() ? Subtotal + Shipping : 0;
        }

        // ========================== SESSION HELPERS ==========================
        private List<Line> GetCartRaw()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            return string.IsNullOrEmpty(json)
                ? new List<Line>()
                : JsonSerializer.Deserialize<List<Line>>(json) ?? new List<Line>();
        }

        private void SaveCartRaw(List<Line> cart)
        {
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));
        }

        private CartVM BuildCartVM()
        {
            var lines = GetCartRaw();
            var ids = lines.Select(l => l.ProductId).ToList();

            var products = _db.Products.AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p);

            var vm = new CartVM();

            foreach (var l in lines)
            {
                if (!products.TryGetValue(l.ProductId, out var p)) continue;

                vm.Items.Add(new ItemVM
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    ImageUrl = string.IsNullOrWhiteSpace(p.ImageUrl) ? "/images/no-image.png" : p.ImageUrl,
                    UnitPrice = p.Price,
                    Quantity = l.Quantity
                });
            }

            return vm;
        }

        // ========================== ACTIONS ==========================

        // SHOW CART
        public IActionResult Index() => View(BuildCartVM());

        // ADD PRODUCT
        public IActionResult Add(int productId, int? quantity)
        {
            var login = RequireLogin();
            if (login != null) return login;

            int qty = quantity ?? 1;

            var cart = GetCartRaw();
            var line = cart.FirstOrDefault(x => x.ProductId == productId);

            if (line == null)
                cart.Add(new Line { ProductId = productId, Quantity = qty });
            else
                line.Quantity += qty;

            SaveCartRaw(cart);
            HttpContext.Session.Remove(CHECKOUT_SELECTION_KEY);

            TempData["CartMessage"] = "Đã thêm vào giỏ!";
            return RedirectToAction(nameof(Index));
        }

        
        // ===== UPDATE =====
        public IActionResult Update(int productId, int quantity)
        {
            var loginCheck = RequireLogin();
            if (loginCheck != null) return loginCheck;

            var cart = GetCartRaw();
            var line = cart.FirstOrDefault(x => x.ProductId == productId);

            if (line != null)
            {
                // Không cho số lượng < 1
                line.Quantity = Math.Max(1, quantity);
                SaveCartRaw(cart);
            }

            HttpContext.Session.Remove(CHECKOUT_SELECTION_KEY);
            return RedirectToAction(nameof(Index));
        }


        // REMOVE PRODUCT
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var login = RequireLogin();
            if (login != null) return login;

            var cart = GetCartRaw();
            cart.RemoveAll(x => x.ProductId == productId);

            SaveCartRaw(cart);
            HttpContext.Session.Remove(CHECKOUT_SELECTION_KEY);

            TempData["CartMessage"] = "Đã xoá sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        // CLEAR CART
        [HttpPost]
        public IActionResult Clear()
        {
            var login = RequireLogin();
            if (login != null) return login;

            SaveCartRaw(new List<Line>());
            HttpContext.Session.Remove(CHECKOUT_SELECTION_KEY);

            return RedirectToAction(nameof(Index));
        }

        // SELECT ITEMS FOR CHECKOUT
        [HttpPost]
        public IActionResult CheckoutSelected(int[] selectedIds)
        {
            var login = RequireLogin();
            if (login != null) return login;

            if (selectedIds.Length == 0)
            {
                TempData["CartError"] = "Bạn chưa chọn sản phẩm nào để thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetString(
                CHECKOUT_SELECTION_KEY,
                JsonSerializer.Serialize(selectedIds.ToList())
            );

            return RedirectToAction("Checkout", "Orders");
        }
    }
}

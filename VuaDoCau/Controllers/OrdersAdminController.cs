using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersAdminController : Controller
    {
        private readonly VuaDoCauDbContext _db;

        public OrdersAdminController(VuaDoCauDbContext db)
        {
            _db = db;
        }

        // ==========================
        // LIST ĐƠN HÀNG
        // ==========================
        public IActionResult Index()
        {
            var orders = _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToList();

            return View(orders);
        }

        // ==========================
        // MARK SHIPPING (GIAO HÀNG)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkShipping(int id)
        {
            var order = _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            var status = (order.Status ?? "").Trim();
            bool isPending =
                status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Chờ xác nhận", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Cho xac nhan", StringComparison.OrdinalIgnoreCase);

            if (isPending)
            {
                // ================================
                // 🌟 TRỪ SỐ LƯỢNG TỒN KHO
                // ================================
                foreach (var item in order.Items)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock -= item.Quantity;

                        // tránh âm số lượng
                        if (item.Product.Stock < 0)
                            item.Product.Stock = 0;
                    }
                }

                // Cập nhật trạng thái
                order.Status = "Shipping";

                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // CANCEL ORDER (HỦY ĐƠN)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var order = _db.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            var status = (order.Status ?? "").Trim();
            bool canCancel =
                !status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                !status.Equals("Shipping", StringComparison.OrdinalIgnoreCase);

            if (canCancel)
            {
                // Xóa item trước
                if (order.Items?.Count > 0)
                    _db.OrderItems.RemoveRange(order.Items);

                // Xóa đơn
                _db.Orders.Remove(order);

                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

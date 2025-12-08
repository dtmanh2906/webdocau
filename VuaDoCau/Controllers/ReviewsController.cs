using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    [Authorize]                       // bắt buộc login để kiểm soát 1 lần/1 sản phẩm
    [Route("reviews")]
    public class ReviewsController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        public ReviewsController(VuaDoCauDbContext db) => _db = db;

        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int stars, string? comment, int? backOrderId)
        {
            stars = Math.Clamp(stars, 1, 5);

            var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId);
            if (p == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var existed = await _db.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existed != null)
            {
                // ----- PHƯƠNG ÁN 1: TỪ CHỐI -----
                // TempData["Msg"] = "Bạn đã đánh giá sản phẩm này rồi.";
                // return backOrderId.HasValue
                //     ? RedirectToAction("Index", "Profile")
                //     : RedirectToAction("Details", "Products", new { id = productId });

                // ----- PHƯƠNG ÁN 2: CẬP NHẬT LẠI (giữ 1 bản duy nhất) -----
                existed.Stars = stars;
                existed.Comment = string.IsNullOrWhiteSpace(comment) ? existed.Comment : comment!.Trim();
                existed.CreatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                TempData["Msg"] = "Cập nhật đánh giá thành công!";
                return backOrderId.HasValue
                    ? RedirectToAction("Index", "Profile")
                    : RedirectToAction("Details", "Products", new { id = productId });
            }

            // Chưa có → tạo mới
            _db.Reviews.Add(new Review
            {
                ProductId = productId,
                Stars = stars,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Cảm ơn bạn đã đánh giá!";
            return backOrderId.HasValue
                ? RedirectToAction("Index", "Profile")
                : RedirectToAction("Details", "Products", new { id = productId });
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VuaDoCau.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string? Sku { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Slug { get; set; }

        // Mô tả ngắn
        public string? Summary { get; set; }

        public string? ImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        // FK
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // ⭐ Thêm các cột phục vụ thống kê/bán hàng
        public int Purchased { get; set; } = 0;   // tổng số lượng đã bán
        public int Stock { get; set; } = 0; 
    }
}

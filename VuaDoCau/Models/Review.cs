using System;
using System.ComponentModel.DataAnnotations;

namespace VuaDoCau.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Stars { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Nếu bạn dùng Identity thì có thể lưu UserId (tuỳ chọn)
        public string? UserId { get; set; }
    }
}

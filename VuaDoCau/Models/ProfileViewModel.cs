namespace VuaDoCau.Models
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public List<OrderVM> Orders { get; set; } = new();
    }

    public class OrderVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
        public decimal Total { get; set; }
        public List<OrderItemVM> Items { get; set; } = new();
    }

    public class OrderItemVM
    {
        public int ProductId { get; set; }                            // <-- THÊM
        public string ProductName { get; set; } = "(Sản phẩm)";
        public int Quantity { get; set; }
    }
}

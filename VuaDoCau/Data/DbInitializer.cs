using VuaDoCau.Models;

namespace VuaDoCau.Data;

public static class DbInitializer
{
    public static void Seed(VuaDoCauDbContext db)
    {
        if (db.Categories.Any()) return;

        var cats = new[]
        {
            new Category { Slug = "can-cau", Name = "Cần câu" },
            new Category { Slug = "may-cau", Name = "Máy câu" },
            new Category { Slug = "day-moi-phu-kien", Name = "Dây / Mồi / Phụ kiện" }
        };
        db.Categories.AddRange(cats);
        db.SaveChanges();

        // Hàm tạo sản phẩm tự set Purchased = 0
        Product P(string sku, string name, string slug, string summary,
            string img, decimal price, int catId, int stock, decimal? oldPrice = null)
        {
            return new Product
            {
                Sku = sku,
                Name = name,
                Slug = slug,
                Summary = summary,
                ImageUrl = img,
                Price = price,
                OldPrice = oldPrice,
                CategoryId = catId,
                Purchased = 0,
                Stock = stock
            };
        }

        var products = new List<Product>
        {
            // -------------------------
            // CẦN CÂU (Stock 25–60)
            // -------------------------
            P("CAN001","Cần Shimano Bassterra 2.4m","can-shimano-bassterra-24",
                "Carbon, 2 khúc, ném bờ","/images/can1.jpg",1790000,cats[0].Id, stock: 40, oldPrice:1990000),

            P("CAN002","Cần Daiwa Crossfire 2.1m","can-daiwa-crossfire-21",
                "Cần carbon 2 khúc, giá rẻ","/images/can1.jpg",890000,cats[0].Id, stock: 55, oldPrice:990000),

            P("CAN003","Cần Shimano Sojourn 2.4m","can-shimano-sojourn-24",
                "Thiết kế cổ điển","/images/can3.jpg",1290000,cats[0].Id, stock: 33),

            P("CAN004","Cần Abu Garcia Veritas 2.7m","can-abu-veritas-27",
                "Siêu nhẹ, dùng lure","/images/can4.jpg",2450000,cats[0].Id, stock: 28),

            P("CAN005","Cần Shimano Zodias 2024 2.1m","can-shimano-zodias-21",
                "Cần cao cấp chuyên lure","/images/can5.jpg",5200000,cats[0].Id, stock: 20, oldPrice:5500000),

            P("CAN006","Cần Pioneer Cobra 2.1m","can-pioneer-cobra-21",
                "Câu cá chép, cá trê","/images/can6.jpg",790000,cats[0].Id, stock: 60),

            // -------------------------
            // MÁY CÂU (Stock 15–40)
            // -------------------------
            P("MAY001","Máy Daiwa Revros 2500","may-daiwa-revros-2500",
                "Spinning, bền nhẹ","/images/may1.jpg",1490000,cats[1].Id, stock: 32),

            P("MAY002","Máy Shimano FX 2500","may-shimano-fx-2500",
                "Tốc độ ổn định","/images/may-2.jpg",890000,cats[1].Id, stock: 40),

            P("MAY003","Máy Daiwa Exceler LT 3000","may-daiwa-exceler-3000",
                "Thân carbon nhẹ","/images/may-3.jpg",1950000,cats[1].Id, stock: 21, oldPrice:2150000),

            P("MAY004","Máy Penn Battle III 4000","may-penn-battle-iii-4000",
                "Siêu khỏe câu biển","/images/may-4.jpg",3200000,cats[1].Id, stock: 18),

            P("MAY005","Máy Shimano Stradic FL 2500","may-shimano-stradic-fl-2500",
                "Hagane Gear bền bỉ","/images/may-5.jpg",4250000,cats[1].Id, stock: 15, oldPrice:4550000),

            P("MAY006","Máy Okuma Ceymar 1000","may-okuma-ceymar-1000",
                "Nhỏ gọn, 7 vòng bi","/images/may-6.jpg",990000,cats[1].Id, stock: 36),

            // -------------------------
            // DÂY, MỒI, PHỤ KIỆN (Stock 80–200)
            // -------------------------
            P("DAY001","Dây PE X4 150m","day-pe-x4-150",
                "Bền, chống mài mòn","/images/day1.jpg",189000,cats[2].Id, stock: 150),

            P("DAY002","Dây fluorocarbon 100m","day-fluorocarbon-100m",
                "Trong suốt, chịu lực","/images/day-2.jpg",159000,cats[2].Id, stock: 130),

            P("DAY003","Dây dù PE 8X YGK 150m","day-pe8x-ygk-150m",
                "8 lõi cao cấp","/images/day-3.jpg",329000,cats[2].Id, stock: 95, oldPrice:379000),

            P("DAY004","Mồi giả Jump Frog","moi-gia-nhai-jumpfrog",
                "Câu cá lóc, cá trê","/images/moi-1.jpg",59000,cats[2].Id, stock: 200),

            P("DAY005","Hộp đựng mồi lure","hop-dung-moi-da-nang",
                "12 ngăn, chống nước","/images/phukien-1.jpg",129000,cats[2].Id, stock: 120),

            P("DAY006","Kìm gỡ cá inox 15cm","kim-go-ca-inox-15",
                "Chống gỉ, an toàn","/images/phukien-2.jpg",99000,cats[2].Id, stock: 180),

            P("DAY007","Lưỡi câu Mustad số 8","luoi-cau-mustad-8",
                "Thép đen, cực bén","/images/phukien-3.jpg",49000,cats[2].Id, stock: 160),

            P("DAY008","Túi cần câu du lịch 1.2m","tui-can-cau-12m",
                "3 ngăn, chống nước","/images/phukien4.jpg",259000,cats[2].Id, stock: 85, oldPrice:289000),
        };

        db.Products.AddRange(products);
        db.SaveChanges();
    }
}

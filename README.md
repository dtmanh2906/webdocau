# Vua Đồ Câu - Website Bán Đồ Câu Cá

## Giới thiệu

**Vua Đồ Câu** là website thương mại điện tử chuyên kinh doanh các sản phẩm phục vụ câu cá như cần câu, máy câu, dây câu, mồi câu và các phụ kiện khác.

Website được xây dựng bằng **ASP.NET Core MVC**, sử dụng **Entity Framework Core** kết hợp **SQL Server** để quản lý dữ liệu và **ASP.NET Identity** để xác thực người dùng.

---

## Công nghệ sử dụng

### Backend
- ASP.NET Core MVC
- C#
- Entity Framework Core
- ASP.NET Identity

### Frontend
- HTML5
- CSS3
- Bootstrap 5
- JavaScript

### Database
- SQL Server

---

# Chức năng của hệ thống

## Người dùng

### Đăng ký tài khoản
- Đăng ký tài khoản mới
- Kiểm tra dữ liệu hợp lệ
- Mật khẩu được mã hóa bởi ASP.NET Identity

### Đăng nhập / Đăng xuất
- Xác thực tài khoản
- Ghi nhớ đăng nhập
- Phân quyền User và Admin

### Xem sản phẩm
- Hiển thị danh sách sản phẩm
- Xem chi tiết sản phẩm
- Hiển thị hình ảnh
- Giá bán
- Số lượt mua
- Đánh giá

### Tìm kiếm sản phẩm
- Tìm theo tên
- Tìm theo danh mục

### Lọc sản phẩm
- Theo danh mục
- Theo giá
- Theo sản phẩm mới nhất

### Giỏ hàng
- Thêm sản phẩm
- Cập nhật số lượng
- Xóa sản phẩm
- Xóa toàn bộ giỏ hàng
- Thanh toán các sản phẩm được chọn
- Thanh toán toàn bộ

### Đặt hàng
- Nhập thông tin nhận hàng
- Tạo đơn hàng
- Theo dõi trạng thái đơn

### Quản lý hồ sơ
- Cập nhật thông tin cá nhân
- Xem lịch sử mua hàng
- Hủy đơn hàng khi chưa xác nhận
- Xác nhận đã nhận hàng

### Đánh giá sản phẩm
- Đánh giá từ 1 đến 5 sao
- Bình luận sản phẩm
- Mỗi tài khoản chỉ đánh giá một lần cho mỗi sản phẩm
- Có thể cập nhật đánh giá

---

## Quản trị viên (Admin)

### Quản lý sản phẩm
- Thêm sản phẩm
- Sửa sản phẩm
- Xóa sản phẩm
- Upload hình ảnh
- Quản lý tồn kho
- Sinh Slug tự động

### Quản lý đơn hàng
- Xem danh sách đơn hàng
- Xác nhận giao hàng
- Hủy đơn hàng
- Cập nhật trạng thái đơn hàng

### Quản lý kho
- Theo dõi số lượng tồn
- Cập nhật số lượng sau khi xác nhận giao hàng

---

# Cơ sở dữ liệu

Hệ thống gồm các bảng chính:

+AspNetUsers : Lưu thông tin tài khoản 
+AspNetRoles : Phân quyền  
+Categories : Danh mục sản phẩm  
+Products : Thông tin sản phẩm  
+Orders : Thông tin đơn hàng  
+OrderItems : Chi tiết đơn hàng 
+Reviews : Đánh giá sản phẩm  


---

# Quy trình mua hàng

1. Đăng ký tài khoản
2. Đăng nhập
3. Chọn sản phẩm
4. Thêm vào giỏ hàng
5. Thanh toán
6. Tạo đơn hàng
7. Admin xác nhận giao hàng
8. Người dùng xác nhận đã nhận hàng
9. Đánh giá sản phẩm

---

# Cấu trúc Project

```

VuaDoCau

│

├── Controllers

│   |── AccountController

│   ├── CartController

│   ├── HomeController

│   ├── OrdersController

│   ├── OrdersAdminController

│   ├── ProductsController

│   ├── ProductsAdminController

│   ├── ProfileController

│   └── ReviewsController

│

├── Models

│

├── Data

│

├── Views

│

├── wwwroot

│   ├── css

│   ├── js

│   ├── images

│

└── Program.cs

```

---

# Cài đặt

## 1. Clone Project

```bash

git clone https://github.com/your-account/VuaDoCau.git

```

## 2. Mở bằng Visual Studio

Chọn:

```

Open Project/Solution

```

---

## 3. Cấu hình Database

Sửa chuỗi kết nối trong:

```

appsettings.json

```

Ví dụ:

```json

"ConnectionStrings": {

&#x20; "DefaultConnection": "Server=.;Database=VuaDoCau;Trusted\_Connection=True;TrustServerCertificate=True;"

}

```

---

## 4. Migration

```powershell

Update-Database

```

Hoặc

```bash

dotnet ef database update

```

---

## 5. Chạy Project

```bash

dotnet run

```

Hoặc nhấn

```

F5

```


---

# Điểm nổi bật

- Giao diện thân thiện
- Phân quyền Admin/User
- Quản lý giỏ hàng bằng Session
- Quản lý đơn hàng
- Đánh giá sản phẩm
- Quản lý tồn kho
- Upload hình ảnh
- Tìm kiếm và lọc sản phẩm
- Thiết kế theo mô hình MVC

---

# Hướng phát triển

- Thanh toán trực tuyến (VNPay, MoMo)
- Quản lý mã giảm giá
- Chat với khách hàng
- Thống kê doanh thu
- Dashboard quản trị
- Yêu thích sản phẩm
- Email xác nhận đơn hàng
- Theo dõi vận chuyển

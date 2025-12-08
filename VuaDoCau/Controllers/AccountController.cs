using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using VuaDoCau.Models;
namespace VuaDoCau.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;

        public AccountController(UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signInMgr)
        {
            _userMgr = userMgr;
            _signInMgr = signInMgr;
        }

        // ---------------- Register ----------------

        [HttpGet]
        public IActionResult Register() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                PhoneNumber = vm.Phone,
                Address = vm.Address,
                EmailConfirmed = true
            };

            var rs = await _userMgr.CreateAsync(user, vm.Password);
            if (rs.Succeeded)
            {
                await _userMgr.AddToRoleAsync(user, "User");
                await _signInMgr.SignInAsync(user, isPersistent: true);
                TempData["Success"] = "Đăng ký tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }

            // Đổ lỗi của Identity (trùng email, mật khẩu yếu, v.v.) ra view
            foreach (var e in rs.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            TempData["Error"] = "Đăng ký thất bại. Vui lòng kiểm tra thông tin.";
            return View(vm);
        }

        // ---------------- Login ----------------

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // chặn cache trang login để tránh trình duyệt giữ lại form cũ
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            return View(new LoginVm { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            // chặn cache response sau post
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";

            if (!ModelState.IsValid) return View(vm);

            var rs = await _signInMgr.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
            if (rs.Succeeded)
            {
                TempData["Success"] = "Đăng nhập thành công!";
                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Sai email hoặc mật khẩu.");
            TempData["Error"] = "Đăng nhập thất bại. Vui lòng thử lại.";
            return View(vm);
        }

        // ---------------- Logout ----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInMgr.SignOutAsync();

            // chặn cache trang trước đó khi back
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";

            TempData["Success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => Content("Bạn không có quyền truy cập.");
    }

    // ---------------- ViewModels ----------------



public record RegisterVm
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string FullName { get; set; } = "";

        [Display(Name = "SĐT")]
        [StringLength(20)]
        [Required(ErrorMessage ="Vui lòng nhập Sdt")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [StringLength(200)]
        [Required(ErrorMessage ="Vui lòng nhập địa chỉ")]
        public string Address { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }


    public record LoginVm
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}

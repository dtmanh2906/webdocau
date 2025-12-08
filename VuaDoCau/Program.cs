using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Models;
using VuaDoCau.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext - SQL Server
builder.Services.AddDbContext<VuaDoCauDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<VuaDoCauDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/AccessDenied";
});

// Session + Cart (dùng Session để đếm số lượng giỏ)
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache(); // backing store cho Session

builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// DI cho CartService (nếu bạn đang dùng)
builder.Services.AddScoped<CartService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session phải bật trước khi đi vào pipeline MVC/Endpoints
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tự động migrate + seed dữ liệu mẫu và tài khoản Admin
using (var scope = app.Services.CreateScope())
{
    var sv = scope.ServiceProvider;
    var db = sv.GetRequiredService<VuaDoCauDbContext>();
    db.Database.Migrate();
    DbInitializer.Seed(db);
    await SeedIdentityAsync(sv);
}

app.Run();

static async Task SeedIdentityAsync(IServiceProvider sv)
{
    var roleMgr = sv.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = sv.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var r in new[] { "Admin", "User" })
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));

    var adminEmail = "admin@vuadocau.local";
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin == null) 
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Site Admin",
            Address = "HN",
            PhoneNumber = "0900000000",
            EmailConfirmed = true
        };
        await userMgr.CreateAsync(admin, "Admin@123");
        await userMgr.AddToRoleAsync(admin, "Admin");
    }
}

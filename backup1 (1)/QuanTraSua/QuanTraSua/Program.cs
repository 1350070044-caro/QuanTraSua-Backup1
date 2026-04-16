using Microsoft.EntityFrameworkCore;
using QuanTraSua.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. PHẢI CÓ DÒNG NÀY: Đăng ký dịch vụ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng tồn tại 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. THỨ TỰ MIDDLEWARE (Cực kỳ quan trọng)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. PHẢI CÓ DÒNG NÀY: Kích hoạt Session (Đặt sau UseRouting và trước UseAuthorization)
app.UseSession(); 

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 

app.Run();

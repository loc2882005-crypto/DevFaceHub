using DevFaceHub.Data;
using DevFaceHub.Models;
using Microsoft.AspNetCore.Identity; // Cần thêm dòng này
using Microsoft.EntityFrameworkCore;

//Progam.cs là file bắt đầu chạy của ứng dụng ASP.NET Core
namespace DevFaceHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //tạo builder
            var builder = WebApplication.CreateBuilder(args);//tạo ứng dụng ASP.NET Core với cấu hình mặc định

            // 1. Đọc chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // 2. Đăng ký ApplicationDbContext
            //khi code cần database, ASP.NET sẽ tự tạo ApplicationDbContext cho mình
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 3. Đăng ký Identity (Sửa AddDefaultIdentity thành AddIdentity)
            //đăng ký hệ thống xác thực người dùng
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Tùy chỉnh một chút để lúc code test đăng nhập/đăng ký cho dễ thở
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()//lưu thông tin user/role vào database theo DbContext
            .AddDefaultTokenProviders(); // Thêm dòng này để hỗ trợ tạo token (quên mật khẩu, v.v.)

            // Add services to the container.
            //đăng ký MVC
            builder.Services.AddControllersWithViews();

            //build app
            //sau khi đăng ký services và config xong, ta build ứng dụng
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // Đảm bảo UseAuthentication nằm trước UseAuthorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            
            //chạy app
            app.Run();

        }
    }
}
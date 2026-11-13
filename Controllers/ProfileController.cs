using DevFaceHub.Data;
using DevFaceHub.Models;
using DevFaceHub.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevFaceHub.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile/Index/{id?}
        public async Task<IActionResult> Index(string? id)
        {
            // 1. Xác định ID của người cần xem trang cá nhân
            var targetUserId = id;
            if (string.IsNullOrEmpty(targetUserId))
            {
                // Nếu không có ID truyền vào, lấy ID của người đang đăng nhập
                targetUserId = _userManager.GetUserId(User);

                // Nếu chưa đăng nhập mà đòi vào Profile trống -> Bắt đi đăng nhập
                if (targetUserId == null) return RedirectToAction("Login", "Account");
            }

            // 2. Lấy thông tin User từ Database
            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null) return NotFound("Không tìm thấy người dùng này.");

            // 3. Lấy danh sách bài viết của riêng User này
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .Where(p => p.UserId == targetUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // 4. Tính tổng số Like mà người này nhận được
            var totalLikes = posts.Sum(p => p.Likes.Count);

            // 5. Đóng gói vào ViewModel
            var model = new ProfileViewModel
            {
                UserInfo = user,
                UserPosts = posts,
                TotalLikesReceived = totalLikes
            };

            return View(model);
        }
    }
}
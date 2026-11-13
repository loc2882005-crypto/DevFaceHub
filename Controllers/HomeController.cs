using DevFaceHub.Data;
using DevFaceHub.Models;
using DevFaceHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevFaceHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment; // Thêm dòng này

        // Cập nhật hàm khởi tạo để nhận IWebHostEnvironment
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // ... (Giữ nguyên hàm Index)

        // GET: /Home/Index (Hiển thị Bảng tin)
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách bài viết từ DB, sắp xếp mới nhất lên đầu, kèm theo thông tin User đăng bài
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // GET: /Home/Cv
        public IActionResult Cv()
        {
            return View();
        }

        // POST: /Home/CreatePost (Xử lý khi người dùng đăng bài mới)
        [HttpPost]
        [Authorize] // Bắt buộc phải đăng nhập mới được đăng bài
        
        public async Task<IActionResult> CreatePost(PostCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return RedirectToAction("Login", "Account");

                string? uploadedImageUrl = null;

                // Nếu người dùng có chọn file ảnh
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Đường dẫn tới thư mục wwwroot/images/posts
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "posts");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file ngẫu nhiên để tránh trùng tên ảnh cũ
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Ghi file vật lý vào ổ cứng máy chủ
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn tương đối để gắn vào thẻ <img src="...">
                    uploadedImageUrl = "/images/posts/" + uniqueFileName;
                }

                var post = new Post
                {
                    Content = model.Content,
                    ImageUrl = uploadedImageUrl, // Lưu đường dẫn vừa xử lý
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = _userManager.GetUserId(User);
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            bool isLiked = false;

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike); // Hủy Like
            }
            else
            {
                _context.Likes.Add(new Like { PostId = postId, UserId = userId }); // Thêm Like
                isLiked = true;
            }

            await _context.SaveChangesAsync();

            // Đếm lại tổng số like hiện tại của bài viết
            var totalLikes = await _context.Likes.CountAsync(l => l.PostId == postId);

            // Trả về dữ liệu ngầm cho JavaScript xử lý
            return Json(new { isLiked = isLiked, totalLikes = totalLikes });
        }

        // POST: /Home/AddComment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string commentContent)
        {
            if (!string.IsNullOrWhiteSpace(commentContent))
            {
                var userId = _userManager.GetUserId(User);
                var comment = new Comment
                {
                    PostId = postId,
                    UserId = userId,
                    Content = commentContent,
                    CreatedAt = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST: /Home/DeletePost
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            // 1. Tìm bài viết trong Database
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null) return NotFound();

            // 2. Bảo mật: Chỉ cho phép chính người đăng bài mới được quyền xóa
            if (post.UserId != currentUser.Id)
            {
                // Nếu ai đó cố tình hack sửa HTML để xóa bài người khác, chặn lại ngay
                return Forbid();
            }

            // 3. Xóa file ảnh trong thư mục wwwroot (nếu bài viết có ảnh)
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                // post.ImageUrl có dạng "/images/posts/abc.jpg", ta cần bỏ dấu "/" ở đầu
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // 4. Xóa bài viết khỏi Database (các Comment và Like của bài này sẽ tự động bị xóa theo do khóa ngoại Cascade)
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // POST: /Home/EditPost
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditPost(int postId, string editContent, IFormFile editImageFile)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            // 1. Tìm bài viết cần sửa
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null) return NotFound();

            // 2. Bảo mật: Chỉ chủ bài viết mới được quyền sửa
            if (post.UserId != currentUser.Id) return Forbid();

            // 3. Cập nhật nội dung chữ
            post.Content = editContent;

            // 4. Xử lý ảnh (Nếu người dùng chọn upload ảnh mới để thay thế)
            if (editImageFile != null && editImageFile.Length > 0)
            {
                // Xóa ảnh cũ vật lý trong ổ cứng để dọn rác
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Lưu ảnh mới
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "posts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + editImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await editImageFile.CopyToAsync(fileStream);
                }

                post.ImageUrl = "/images/posts/" + uniqueFileName;
            }

            // 5. Lưu vào Database
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
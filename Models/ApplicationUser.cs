using Microsoft.AspNetCore.Identity;

namespace DevFaceHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        // Thêm trường ngày tham gia (Mặc định lấy thời điểm tạo)
        public DateTime JoinDate { get; set; } = DateTime.Now;

        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
    }
}
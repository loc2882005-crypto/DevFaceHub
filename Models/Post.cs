using System.ComponentModel.DataAnnotations;

namespace DevFaceHub.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết không được để trống")]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết với ApplicationUser (Kiểu string)
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Navigation properties cho Comment và Like
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace DevFaceHub.Models
{
    public class Like
    {
        [Key]
        public int LikeId { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
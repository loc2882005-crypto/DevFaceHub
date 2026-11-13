using DevFaceHub.Models;

namespace DevFaceHub.ViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser UserInfo { get; set; }
        public IEnumerable<Post> UserPosts { get; set; }
        public int TotalLikesReceived { get; set; }
    }
}
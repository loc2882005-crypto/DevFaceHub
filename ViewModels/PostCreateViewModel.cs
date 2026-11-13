using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DevFaceHub.ViewModels
{
    public class PostCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết")]
        public string Content { get; set; }

        // Dùng IFormFile để nhận file trực tiếp từ thẻ <input type="file">
        public IFormFile? ImageFile { get; set; }
    }
}
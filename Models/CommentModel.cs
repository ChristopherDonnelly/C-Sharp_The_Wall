using System;
using System.ComponentModel.DataAnnotations;

namespace The_Wall.Models
{
    public class Comment
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int MessageId { get; set; }
        public int CommentId { get; set; }
        
        [Required(ErrorMessage = "Comment is required!")]
        [Display(Name = "Post a Comment: ")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
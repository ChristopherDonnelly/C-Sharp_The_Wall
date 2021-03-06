using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace The_Wall.Models
{
    public class Message
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int MessageId { get; set; }
        
        [Required(ErrorMessage = "Message is required!")]
        [Display(Name = "Post a Message: ")]
        public string MessageContent { get; set; }
        public List<Comment> Comments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
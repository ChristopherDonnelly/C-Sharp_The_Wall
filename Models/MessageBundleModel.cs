using System.Collections.Generic;

namespace The_Wall.Models
{
    public class ModelBundle
    {
        public Message SingleMessage { get; set; }
        public List<Message> AllMessages { get; set; }
    }
}
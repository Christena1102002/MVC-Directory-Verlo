using System;

namespace mvc.ViewModels
{
    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string SenderId { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}

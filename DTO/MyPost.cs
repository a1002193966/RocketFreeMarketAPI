using System;

namespace DTO
{
    public class MyPost
    {
        public int PostID { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public int ViewCount { get; set; }
    }
}

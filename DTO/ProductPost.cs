using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class ProductPost
    {
        public string State { get; set; }
        public string City { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public string Content { get; set; }
        public int UserID { get; set; }
    }
}

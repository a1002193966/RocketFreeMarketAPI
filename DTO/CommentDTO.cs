using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class CommentDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
    }
}

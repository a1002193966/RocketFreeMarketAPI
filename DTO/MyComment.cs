using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO
{
    public class MyComment
    {
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        [Required]
        public int PostID { get; set; }
    }
}

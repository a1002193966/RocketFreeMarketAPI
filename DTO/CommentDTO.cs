using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO
{
    public class CommentDTO
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime CommentDate { get; set; }
    }
}

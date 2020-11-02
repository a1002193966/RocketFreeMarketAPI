using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO
{
    public class ProductPost
    {
        [Required]
        [StringLength(20)]
        public string State { get; set; }
        [Required]
        [StringLength(25)]
        public string City { get; set; }
        [Required]
        [StringLength(100)]
        public string Subject { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        [Range(0, 9999999999999999.99)]
        public decimal Price { get; set; }
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        [Required]
        public int UserID { get; set; }
    }
}

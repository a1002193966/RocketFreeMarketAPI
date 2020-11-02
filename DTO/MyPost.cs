using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class MyPost
    {
        [Required]
        public int PostID { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime LastUpdateDate { get; set; }
        [Required]
        [StringLength(25)]
        public string City { get; set; }
        [Required]
        [StringLength(20)]
        public string State { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        [Range(0, 9999999999999999.99)]
        public decimal Price { get; set; }
        [Required]
        [StringLength(100)]
        public string Subject { get; set; }
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        [Required]
        [Range(0, 99999999999)]
        public int ViewCount { get; set; }
    }
}

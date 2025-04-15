using mvc.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Ad
    {
        [Key]
        public int Id { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public AdType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PackageType PackageType { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("Business")]
        public int? BusinessId { get; set; }

        public  Business? Business { get; set; }

      
    }

    
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace KuShop.Models
{
    public partial class Customer
    {
        [Key]
   
        
        public string CusId { get; set; } = null!;
        [Display(Name = "ชื่อ - นามสกุล")]
        public string CusName { get; set; } = null!;
        [Display(Name = "ชื่อผู้ใช้งาน")]
        public string CusLogin { get; set; } = null!;
        [Display(Name = "รหัสผ่าน")]
        public string CusPass { get; set; } = null!;
        [Display(Name = "อีเมล")]
        public string CusEmail { get; set; } = null!;
        public string? CusAdd { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}

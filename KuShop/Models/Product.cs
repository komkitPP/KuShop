using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace KuShop.Models
{
    public partial class Product
    {
        [Key]
        [Required(ErrorMessage = "ต้องระบุรหัสสินค้า")]
        [Display(Name = "รหัสสินค้า")]
        public string PdId { get; set; } = null!;

        [Required(ErrorMessage = "ต้องระบุชื่อสินค้า")]
        [StringLength(100)]
        [Display(Name = "ชื่อสินค้า")]
        public string PdName { get; set; } = null!;

        [Display(Name = "ประเภท")]
        public byte? PdtId { get; set; }

        [Display(Name = "ยี่ห้อ")]
        public byte? BrandId { get; set; }

        [Display(Name = "ราคาขาย")]
        [Range(0, 1000000, ErrorMessage = "ราคาขายต้องอยู่ระหว่าง 0-1,000,000")]
        public double? PdPrice { get; set; }

        [Display(Name = "ราคาต้นทุน")]
        [Range(0, 1000000, ErrorMessage = "ราคาขายต้องอยู่ระหว่าง 0-1,000,000")]
        public double? PdCost { get; set; }

        [Display(Name = "ยอดคงเหลือ")]
        public double? PdStk { get; set; }

        [Display(Name = "วันที่ซื้อเข้าครั้งสุดท้าย")]
        public DateTime? PdLastBuy { get; set; }

        [Display(Name = "วันที่ขายออกครั้งสุดท้าย")]
        public DateTime? PdLastSale { get; set; }

        [Display(Name = "ยังขายอยู่")]
        public string? Active { get; set; }
    }
}

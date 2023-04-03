using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace KuShop.Models
{
    public partial class Buying
    {
        [Key]
        [Required(ErrorMessage = "ต้องระบุรหัสสินค้า")]
        [Display(Name = "รหัสสินค้า")]
        public string BuyId { get; set; } = null!;
        public string? SupId { get; set; }
        public DateTime? BuyDate { get; set; }
        public string? StfId { get; set; }
        public string? BuyDocId { get; set; }
        public string? Saleman { get; set; }
        public double? BuyQty { get; set; }
        public double? BuyMoney { get; set; }
        public string? BuyRemark { get; set; }
    }
}



using System.ComponentModel.DataAnnotations;

namespace KuShop.ViewModels
{
    public class PdVM
    {
        [Key]
        public string PdId { get; set; } = null!;
        public string PdName { get; set; } = null!;
        public string? PdtName { get; set; }
        public string? BrandName { get; set; }
        public double? PdPrice { get; set; }
        public double? PdCost { get; set; }
        public double? PdStk { get; set; }
        public DateTime? PdLastBuy { get; set; }
        public DateTime? PdLastSale { get; set; }
    }
}

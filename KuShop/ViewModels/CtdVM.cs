﻿namespace KuShop.ViewModels
{
    public class CtdVM
    {
        public string CartId { get; set; } = null!;
        public string PdId { get; set; } = null!;
        public string PdName { get; set; } = null!;
        public double? CdtlQty { get; set; }
        public double? CdtlPrice { get; set; }
        public double? CdtlMoney { get; set; }
    }
}

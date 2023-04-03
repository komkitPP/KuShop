using System;
using System.Collections.Generic;

namespace KuShop.Models
{
    public partial class Staff
    {
        public string StfId { get; set; } = null!;
        public string StfName { get; set; } = null!;
        public string StfPass { get; set; } = null!;
        public string? DutyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? QuitDate { get; set; }
    }
}

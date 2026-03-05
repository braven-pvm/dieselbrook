using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnqIntegrationApi.Models.Nop
{
    [Table("ANQ_Discount_AppliedToCustomers")]
    public class AnqDiscountAppliedToCustomer
    {
        [Key]
        public int Id { get; set; }

        public int DiscountId { get; set; }

        public int CustomerId { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "datetime2(6)")]
        public DateTime? StartDateUtc { get; set; }

        [Column(TypeName = "datetime2(6)")]
        public DateTime? EndDateUtc { get; set; }

        public int DiscountLimitationId { get; set; } = 25;

        public int NoTimesUsed { get; set; } = 0;

        public int LimitationTimes { get; set; } = 1;

        public bool? Notified { get; set; } = false;

        public string? Comment { get; set; }

        public bool? NotifyWhatsApp { get; set; } = false;
    
    }


}

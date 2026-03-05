using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Discount_AppliedToCustomersArchive")]
public partial class AnqDiscountAppliedToCustomersArchive
{
    [Key]
    public int Id { get; set; }

    public int DiscountId { get; set; }

    public int CustomerId { get; set; }

    [StringLength(255)]
    public string? Comment { get; set; }

    public bool? Notified { get; set; }

    public bool IsActive { get; set; }

    [Precision(6)]
    public DateTime? StartDateUtc { get; set; }

    [Precision(6)]
    public DateTime? EndDateUtc { get; set; }

    public int DiscountLimitationId { get; set; }

    public int NoTimesUsed { get; set; }

    public int LimitationTimes { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("AnqDiscountAppliedToCustomersArchives")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("DiscountId")]
    [InverseProperty("AnqDiscountAppliedToCustomersArchives")]
    public virtual Discount Discount { get; set; } = null!;
}

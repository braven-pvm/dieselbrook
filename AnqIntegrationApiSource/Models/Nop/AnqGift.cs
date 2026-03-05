using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Gift")]
public partial class AnqGift
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Sku { get; set; } = null!;

    public int ProductId { get; set; }

    [Column("nQtyLimit")]
    public int NQtyLimit { get; set; }

    [Column("nMinSales", TypeName = "decimal(19, 5)")]
    public decimal? NMinSales { get; set; }

    [Column("cGiftType")]
    [StringLength(10)]
    public string? CGiftType { get; set; }

    public int? CampaignId { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }
}

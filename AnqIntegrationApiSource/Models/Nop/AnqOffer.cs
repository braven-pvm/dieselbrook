using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Offers")]
public partial class AnqOffer
{
    [Key]
    public int Id { get; set; }

    public int? DiscountId { get; set; }

    public string? RuleType { get; set; }

    public int? MaxQty { get; set; }

    public int? MinQty { get; set; }

    [Column(TypeName = "decimal(19, 5)")]
    public decimal? MaxValue { get; set; }

    [Column(TypeName = "decimal(19, 5)")]
    public decimal? MinValue { get; set; }

    public int? PictureId { get; set; }
}

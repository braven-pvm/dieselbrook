using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("OrderItem")]
public partial class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public Guid OrderItemGuid { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal UnitPriceInclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal UnitPriceExclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal PriceInclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal PriceExclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal DiscountAmountInclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal DiscountAmountExclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OriginalProductCost { get; set; }

    public string? AttributeDescription { get; set; }

    public string? AttributesXml { get; set; }

    public int DownloadCount { get; set; }

    public bool IsDownloadActivated { get; set; }

    public int? LicenseDownloadId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? ItemWeight { get; set; }

    [Precision(6)]
    public DateTime? RentalStartDateUtc { get; set; }

    [Precision(6)]
    public DateTime? RentalEndDateUtc { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("OrderItems")]
    public virtual Product Product { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Discount_Usage")]
public partial class AnqDiscountUsage
{
    [Key]
    public int Id { get; set; }

    public int DiscountUsageHistoryId { get; set; }

    public int? DiscountCustomerMappingId { get; set; }

    public int OrderId { get; set; }

    public int? OrderItemId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal DiscountAmount { get; set; }
}

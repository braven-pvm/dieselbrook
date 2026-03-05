using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_ExclusiveItems")]
public partial class AnqExclusiveItem
{
    [Key]
    public int Id { get; set; }

    [Column("ProductID")]
    public int? ProductId { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [Column("RegistrationID")]
    public int? RegistrationId { get; set; }

    [Column("nQtyLimit")]
    public int? NQtyLimit { get; set; }

    [Column("nQtyPurchased")]
    public int? NQtyPurchased { get; set; }

    [Column("dFrom")]
    [Precision(6)]
    public DateTime? DFrom { get; set; }

    [Column("dTo")]
    [Precision(6)]
    public DateTime? DTo { get; set; }

    [Column("IActive")]
    public bool? Iactive { get; set; }

    [Column("IForce")]
    public bool? Iforce { get; set; }

    [Column("IStarter")]
    public bool? Istarter { get; set; }
}

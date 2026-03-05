using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_EventItems")]
public partial class AnqEventItem
{
    [Key]
    public int Id { get; set; }

    [Column("EventID")]
    public int EventId { get; set; }

    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("nQtyLimit")]
    public int NQtyLimit { get; set; }

    [Column("dFrom", TypeName = "datetime")]
    public DateTime DFrom { get; set; }

    [Column("dTo", TypeName = "datetime")]
    public DateTime DTo { get; set; }

    [Column("IActive")]
    public bool Iactive { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? Sku { get; set; }
}

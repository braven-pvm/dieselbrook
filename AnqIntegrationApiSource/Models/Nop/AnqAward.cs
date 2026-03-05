using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Award")]
public partial class AnqAward
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    [StringLength(10)]
    public string? AwardType { get; set; }

    [StringLength(40)]
    public string? Description { get; set; }

    public int MaxValue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpiryDate { get; set; }

    public int? OrderId { get; set; }

    [Column("dcreated", TypeName = "datetime")]
    public DateTime? Dcreated { get; set; }

    [Column("dtaken", TypeName = "datetime")]
    public DateTime? Dtaken { get; set; }
}

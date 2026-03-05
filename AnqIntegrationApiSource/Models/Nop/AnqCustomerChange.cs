using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_CustomerChanges")]
public partial class AnqCustomerChange
{
    [Key]
    public int Id { get; set; }

    public int? ChangeId { get; set; }

    [Column("cTableName")]
    [StringLength(50)]
    public string? CTableName { get; set; }

    public int? CustomerId { get; set; }

    [Column("cCustno")]
    [StringLength(100)]
    public string? CCustno { get; set; }

    [Column("cFieldname")]
    [StringLength(50)]
    public string? CFieldname { get; set; }

    [Column("cOldvalue")]
    public string? COldvalue { get; set; }

    [Column("cNewvalue")]
    public string? CNewvalue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? InsUpdDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Updated { get; set; }
}

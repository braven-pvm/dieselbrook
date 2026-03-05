using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Keyless]
[Table("soxitems")]
public partial class Soxitem
{
    [Column("ID")]
    public int Id { get; set; }

    [Column("cCustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string? CCustno { get; set; }

    [Column("cItemno")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CItemno { get; set; }

    [Column("RegistrationID")]
    public int? RegistrationId { get; set; }

    [Column("nQtyLimit")]
    public int? NQtyLimit { get; set; }

    [Column("nQtyPurchased")]
    public int? NQtyPurchased { get; set; }

    [Column("dFrom", TypeName = "datetime")]
    public DateTime? DFrom { get; set; }

    [Column("dTo", TypeName = "datetime")]
    public DateTime? DTo { get; set; }

    [Column("lActive")]
    public bool? LActive { get; set; }

    [Column("dlastupd", TypeName = "datetime")]
    public DateTime? Dlastupd { get; set; }

    [Column("cuser")]
    [StringLength(20)]
    public string? Cuser { get; set; }

    [Column("lupdatetows")]
    public bool? Lupdatetows { get; set; }

    [Column("lforce")]
    public bool? Lforce { get; set; }
}




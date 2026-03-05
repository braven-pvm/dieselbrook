using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("soskit")]
public partial class Soskit
{
    [Key]
    [Column("cuid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cuid { get; set; } = null!;

    [Column("csono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csono { get; set; } = null!;

    [Column("clineitem")]
    [StringLength(10)]
    [Unicode(false)]
    public string Clineitem { get; set; } = null!;

    [Column("citemno")]
    [StringLength(20)]
    [Unicode(false)]
    public string Citemno { get; set; } = null!;

    [Column("cspeccode1")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cspeccode1 { get; set; } = null!;

    [Column("cspeccode2")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cspeccode2 { get; set; } = null!;

    [Column("cdescript")]
    [StringLength(54)]
    [Unicode(false)]
    public string Cdescript { get; set; } = null!;

    [Column("lprint")]
    public short Lprint { get; set; }

    [Column("lstock")]
    public short Lstock { get; set; }

    [Column("nseq")]
    public int Nseq { get; set; }

    [Column("nqty", TypeName = "numeric(16, 4)")]
    public decimal Nqty { get; set; }

    [Column("ncost", TypeName = "numeric(16, 4)")]
    public decimal Ncost { get; set; }
}




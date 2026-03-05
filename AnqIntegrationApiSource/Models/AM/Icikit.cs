using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("icikit")]
public partial class Icikit
{
    [Key]
    [Column("cuid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cuid { get; set; } = null!;

    [Column("ckititemno")]
    [StringLength(20)]
    [Unicode(false)]
    public string Ckititemno { get; set; } = null!;

    [Column("ckspeccode1")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ckspeccode1 { get; set; } = null!;

    [Column("ckspeccode2")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ckspeccode2 { get; set; } = null!;

    [Column("ccompno")]
    [StringLength(20)]
    [Unicode(false)]
    public string Ccompno { get; set; } = null!;

    [Column("ccspeccode1")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccspeccode1 { get; set; } = null!;

    [Column("ccspeccode2")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccspeccode2 { get; set; } = null!;

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

    [Column("lfree")]
    public short Lfree { get; set; }

    [Column("cgfttype")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cgfttype { get; set; } = null!;

    [Column("cclass")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cclass { get; set; } = null!;

    [Column("nstdcost", TypeName = "numeric(16, 4)")]
    public decimal Nstdcost { get; set; }
}




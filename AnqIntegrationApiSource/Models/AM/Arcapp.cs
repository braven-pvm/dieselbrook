using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("arcapp")]
public partial class Arcapp
{
    [Key]
    [Column("cuid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cuid { get; set; } = null!;

    [Column("ccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccustno { get; set; } = null!;

    [Column("cinvno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cinvno { get; set; } = null!;

    [Column("crcptno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Crcptno { get; set; } = null!;

    [Column("ccinvno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccinvno { get; set; } = null!;

    [Column("cpcustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cpcustno { get; set; } = null!;

    [Column("cccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cccustno { get; set; } = null!;

    [Column("crfndno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Crfndno { get; set; } = null!;

    [Column("caracc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Caracc { get; set; }

    [Column("cdiscacc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Cdiscacc { get; set; }

    [Column("cadjacc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Cadjacc { get; set; }

    [Column("cdebtacc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Cdebtacc { get; set; }

    [Column("cappuid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cappuid { get; set; } = null!;

    [Column("cinvcuid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cinvcuid { get; set; } = null!;

    [Column("crfnduid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Crfnduid { get; set; } = null!;

    [Column("ctogl")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Ctogl { get; set; }

    [Column("dpaid", TypeName = "datetime")]
    public DateTime Dpaid { get; set; }

    [Column("lvoid")]
    public short Lvoid { get; set; }

    [Column("ltemp")]
    public short Ltemp { get; set; }

    [Column("npaidamt", TypeName = "numeric(18, 4)")]
    public decimal Npaidamt { get; set; }

    [Column("ndiscamt", TypeName = "numeric(18, 4)")]
    public decimal Ndiscamt { get; set; }

    [Column("nadjamt", TypeName = "numeric(18, 4)")]
    public decimal Nadjamt { get; set; }

    [Column("ndebtamt", TypeName = "numeric(18, 4)")]
    public decimal Ndebtamt { get; set; }

    [Column("ncbkamt", TypeName = "numeric(18, 4)")]
    public decimal Ncbkamt { get; set; }

    [Column("nmcvaramt", TypeName = "numeric(18, 4)")]
    public decimal Nmcvaramt { get; set; }

    [Column("nfpaidamt", TypeName = "numeric(18, 4)")]
    public decimal Nfpaidamt { get; set; }

    [Column("nfdiscamt", TypeName = "numeric(18, 4)")]
    public decimal Nfdiscamt { get; set; }

    [Column("nfadjamt", TypeName = "numeric(18, 4)")]
    public decimal Nfadjamt { get; set; }

    [Column("nfdebtamt", TypeName = "numeric(18, 4)")]
    public decimal Nfdebtamt { get; set; }

    [Column("nfcbkamt", TypeName = "numeric(18, 4)")]
    public decimal Nfcbkamt { get; set; }
}




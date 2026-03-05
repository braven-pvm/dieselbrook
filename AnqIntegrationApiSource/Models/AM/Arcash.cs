using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("arcash")]
public partial class Arcash
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

    [Column("crcptno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Crcptno { get; set; } = null!;

    [Column("cdepno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cdepno { get; set; } = null!;

    [Column("cpaycode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cpaycode { get; set; } = null!;

    [Column("cbankno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbankno { get; set; } = null!;

    [Column("cchkno")]
    [StringLength(50)]
    [Unicode(false)]
    public string Cchkno { get; set; } = null!;

    [Column("cpayref")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cpayref { get; set; } = null!;

    [Column("ccurrcode")]
    [StringLength(3)]
    [Unicode(false)]
    public string Ccurrcode { get; set; } = null!;

    [Column("ctogl")]
    [StringLength(1)]
    [Unicode(false)]
    public string Ctogl { get; set; } = null!;

    [Column("ctoglmc")]
    [StringLength(1)]
    [Unicode(false)]
    public string Ctoglmc { get; set; } = null!;

    [Column("dcreate", TypeName = "datetime")]
    public DateTime Dcreate { get; set; }

    [Column("dpaid", TypeName = "datetime")]
    public DateTime Dpaid { get; set; }

    [Column("dlastapp", TypeName = "datetime")]
    public DateTime? Dlastapp { get; set; }

    [Column("lvoid")]
    public short Lvoid { get; set; }

    [Column("lprtrcpt")]
    public short Lprtrcpt { get; set; }

    [Column("npaytype")]
    public int Npaytype { get; set; }

    [Column("npaidamt", TypeName = "numeric(18, 4)")]
    public decimal Npaidamt { get; set; }

    [Column("nappamt", TypeName = "numeric(18, 4)")]
    public decimal Nappamt { get; set; }

    [Column("nfpaidamt", TypeName = "numeric(18, 4)")]
    public decimal Nfpaidamt { get; set; }

    [Column("nfappamt", TypeName = "numeric(18, 4)")]
    public decimal Nfappamt { get; set; }

    [Column("ntotmcvar", TypeName = "numeric(18, 4)")]
    public decimal Ntotmcvar { get; set; }

    [Column("nmcround", TypeName = "numeric(18, 4)")]
    public decimal Nmcround { get; set; }

    [Column("nxchgrate", TypeName = "numeric(16, 6)")]
    public decimal Nxchgrate { get; set; }

    [Column("nbpaidamt", TypeName = "numeric(18, 4)")]
    public decimal Nbpaidamt { get; set; }

    [Column("csono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csono { get; set; } = null!;

    [Column("centerby")]
    [StringLength(30)]
    [Unicode(false)]
    public string Centerby { get; set; } = null!;

    [Column("cvresncode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cvresncode { get; set; } = null!;

    [Column("cdepcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cdepcode { get; set; } = null!;

    [Column("lblock")]
    public short Lblock { get; set; }
}




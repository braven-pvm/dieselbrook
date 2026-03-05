using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("sostrs")]
public partial class Sostr
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

    [Column("ccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccustno { get; set; } = null!;

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

    [Column("cwarehouse")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cwarehouse { get; set; } = null!;

    [Column("cmeasure")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cmeasure { get; set; } = null!;

    [Column("ccommiss")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccommiss { get; set; } = null!;

    [Column("crevncode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Crevncode { get; set; } = null!;

    [Column("ctaxcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctaxcode { get; set; } = null!;

    [Column("drequest", TypeName = "datetime")]
    public DateTime? Drequest { get; set; }

    [Column("lkititem")]
    public short Lkititem { get; set; }

    [Column("lupsell")]
    public short Lupsell { get; set; }

    [Column("lstock")]
    public short Lstock { get; set; }

    [Column("lmodikit")]
    public short Lmodikit { get; set; }

    [Column("ltaxable1")]
    public short Ltaxable1 { get; set; }

    [Column("ltaxable2")]
    public short Ltaxable2 { get; set; }

    [Column("lowrmk")]
    public short Lowrmk { get; set; }

    [Column("lptrmk")]
    public short Lptrmk { get; set; }

    [Column("nqtydec")]
    public int Nqtydec { get; set; }

    [Column("ndiscrate", TypeName = "numeric(6, 2)")]
    public decimal Ndiscrate { get; set; }

    [Column("ntaxver", TypeName = "numeric(5, 0)")]
    public decimal Ntaxver { get; set; }

    [Column("nsalesamt", TypeName = "numeric(18, 4)")]
    public decimal Nsalesamt { get; set; }

    [Column("ndiscamt", TypeName = "numeric(18, 4)")]
    public decimal Ndiscamt { get; set; }

    [Column("ntaxamt1", TypeName = "numeric(18, 4)")]
    public decimal Ntaxamt1 { get; set; }

    [Column("ntaxamt2", TypeName = "numeric(18, 4)")]
    public decimal Ntaxamt2 { get; set; }

    [Column("ntaxamt3", TypeName = "numeric(18, 4)")]
    public decimal Ntaxamt3 { get; set; }

    [Column("nfsalesamt", TypeName = "numeric(18, 4)")]
    public decimal Nfsalesamt { get; set; }

    [Column("nfdiscamt", TypeName = "numeric(18, 4)")]
    public decimal Nfdiscamt { get; set; }

    [Column("nftaxamt1", TypeName = "numeric(18, 4)")]
    public decimal Nftaxamt1 { get; set; }

    [Column("nftaxamt2", TypeName = "numeric(18, 4)")]
    public decimal Nftaxamt2 { get; set; }

    [Column("nftaxamt3", TypeName = "numeric(18, 4)")]
    public decimal Nftaxamt3 { get; set; }

    [Column("nbuiltqty", TypeName = "numeric(16, 4)")]
    public decimal Nbuiltqty { get; set; }

    [Column("nordqty", TypeName = "numeric(16, 4)")]
    public decimal Nordqty { get; set; }

    [Column("nshipqty", TypeName = "numeric(16, 4)")]
    public decimal Nshipqty { get; set; }

    [Column("nadvqty", TypeName = "numeric(16, 4)")]
    public decimal Nadvqty { get; set; }

    [Column("nitmcnvqty", TypeName = "numeric(16, 4)")]
    public decimal Nitmcnvqty { get; set; }

    [Column("ntrscnvqty", TypeName = "numeric(16, 4)")]
    public decimal Ntrscnvqty { get; set; }

    [Column("nweight", TypeName = "numeric(16, 2)")]
    public decimal Nweight { get; set; }

    [Column("ncost", TypeName = "numeric(16, 4)")]
    public decimal Ncost { get; set; }

    [Column("nprice", TypeName = "numeric(16, 4)")]
    public decimal Nprice { get; set; }

    [Column("nprcinctx", TypeName = "numeric(16, 4)")]
    public decimal Nprcinctx { get; set; }

    [Column("nfprice", TypeName = "numeric(16, 4)")]
    public decimal Nfprice { get; set; }

    [Column("nfprcinctx", TypeName = "numeric(16, 4)")]
    public decimal Nfprcinctx { get; set; }

    [Column("nseq")]
    public int Nseq { get; set; }

    [Column("nstock", TypeName = "numeric(16, 4)")]
    public decimal Nstock { get; set; }

    [Column("cvernum")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cvernum { get; set; } = null!;

    [Column("ccmpprodln")]
    [StringLength(20)]
    [Unicode(false)]
    public string Ccmpprodln { get; set; } = null!;

    [Column("nbprice", TypeName = "numeric(16, 4)")]
    public decimal Nbprice { get; set; }

    [Column("nextnddprc", TypeName = "numeric(18, 4)")]
    public decimal Nextnddprc { get; set; }

    [Column("lfree")]
    public short Lfree { get; set; }

    [Column("cgfttype")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cgfttype { get; set; } = null!;

    [Column("cinvno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cinvno { get; set; } = null!;

    [Column("cSORefUId")]
    [StringLength(15)]
    [Unicode(false)]
    public string? CSorefUid { get; set; }

    [Column("cSORef")]
    [StringLength(10)]
    [Unicode(false)]
    public string? CSoref { get; set; }

    [Column("csppruid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Csppruid { get; set; } = null!;

    [Column("ccampuid")]
    [StringLength(15)]
    [Unicode(false)]
    public string? Ccampuid { get; set; }

    [Column("lptarpsrmk")]
    public short Lptarpsrmk { get; set; }

    [Column("lptsoplrmk")]
    public short Lptsoplrmk { get; set; }

    [Column("lptsopsrmk")]
    public short Lptsopsrmk { get; set; }

    [Column("lautoacpt")]
    public short Lautoacpt { get; set; }

    [Column("ldropship")]
    public short Ldropship { get; set; }

    [Column("ixitmID")]
    public int IxitmId { get; set; }

    [Column("lcusxitm")]
    public int Lcusxitm { get; set; }
}




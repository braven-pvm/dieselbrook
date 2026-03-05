using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("sosord")]
public partial class Sosord
{
    [Key]
    [Column("csono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csono { get; set; } = null!;

    [Column("crevision")]
    [StringLength(1)]
    [Unicode(false)]
    public string Crevision { get; set; } = null!;

    [Column("ccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccustno { get; set; } = null!;

    [Column("corderby")]
    [StringLength(30)]
    [Unicode(false)]
    public string Corderby { get; set; } = null!;

    [Column("cslpnno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cslpnno { get; set; } = null!;

    [Column("centerby")]
    [StringLength(30)]
    [Unicode(false)]
    public string Centerby { get; set; } = null!;

    [Column("cbaddrno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbaddrno { get; set; } = null!;

    [Column("cbcompany")]
    [StringLength(40)]
    [Unicode(false)]
    public string Cbcompany { get; set; } = null!;

    [Column("cbaddr1")]
    [StringLength(40)]
    [Unicode(false)]
    public string Cbaddr1 { get; set; } = null!;

    [Column("cbaddr2")]
    [StringLength(40)]
    [Unicode(false)]
    public string Cbaddr2 { get; set; } = null!;

    [Column("cbcity")]
    [StringLength(30)]
    [Unicode(false)]
    public string Cbcity { get; set; } = null!;

    [Column("cbstate")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cbstate { get; set; } = null!;

    [Column("cbzip")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbzip { get; set; } = null!;

    [Column("cbcountry")]
    [StringLength(25)]
    [Unicode(false)]
    public string Cbcountry { get; set; } = null!;

    [Column("cbphone")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cbphone { get; set; } = null!;

    [Column("cbcontact")]
    [StringLength(30)]
    [Unicode(false)]
    public string Cbcontact { get; set; } = null!;

    [Column("csaddrno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csaddrno { get; set; } = null!;

    [Column("cscompany")]
    [StringLength(40)]
    [Unicode(false)]
    public string Cscompany { get; set; } = null!;

    [Column("csaddr1")]
    [StringLength(40)]
    [Unicode(false)]
    public string Csaddr1 { get; set; } = null!;

    [Column("csaddr2")]
    [StringLength(40)]
    [Unicode(false)]
    public string Csaddr2 { get; set; } = null!;

    [Column("cscity")]
    [StringLength(30)]
    [Unicode(false)]
    public string Cscity { get; set; } = null!;

    [Column("csstate")]
    [StringLength(15)]
    [Unicode(false)]
    public string Csstate { get; set; } = null!;

    [Column("cszip")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cszip { get; set; } = null!;

    [Column("cscountry")]
    [StringLength(25)]
    [Unicode(false)]
    public string Cscountry { get; set; } = null!;

    [Column("csphone")]
    [StringLength(20)]
    [Unicode(false)]
    public string Csphone { get; set; } = null!;

    [Column("cscontact")]
    [StringLength(30)]
    [Unicode(false)]
    public string Cscontact { get; set; } = null!;

    [Column("cshipvia")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cshipvia { get; set; } = null!;

    [Column("cfob")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cfob { get; set; } = null!;

    [Column("cpono")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cpono { get; set; } = null!;

    [Column("cfrgtcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cfrgtcode { get; set; } = null!;

    [Column("cfrtaxcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cfrtaxcode { get; set; } = null!;

    [Column("ctaxcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctaxcode { get; set; } = null!;

    [Column("cpaycode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cpaycode { get; set; } = null!;

    [Column("cbankno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbankno { get; set; } = null!;

    [Column("cchkno")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cchkno { get; set; } = null!;

    [Column("ccardno")]
    [StringLength(50)]
    [Unicode(false)]
    public string Ccardno { get; set; } = null!;

    [Column("cexpdate")]
    [StringLength(5)]
    [Unicode(false)]
    public string Cexpdate { get; set; } = null!;

    [Column("ccardname")]
    [StringLength(30)]
    [Unicode(false)]
    public string Ccardname { get; set; } = null!;

    [Column("cpayref")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cpayref { get; set; } = null!;

    [Column("ccurrcode")]
    [StringLength(3)]
    [Unicode(false)]
    public string Ccurrcode { get; set; } = null!;

    [Column("ciono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ciono { get; set; } = null!;

    [Column("ccommiss")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccommiss { get; set; } = null!;

    [Column("csource")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csource { get; set; } = null!;

    [Column("cbsono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbsono { get; set; } = null!;

    [Column("dcreate", TypeName = "datetime")]
    public DateTime Dcreate { get; set; }

    [Column("dorder", TypeName = "datetime")]
    public DateTime? Dorder { get; set; }

    [Column("lquote")]
    public short Lquote { get; set; }

    [Column("lhold")]
    public short Lhold { get; set; }

    [Column("lvoid")]
    public short Lvoid { get; set; }

    [Column("lnobo")]
    public short Lnobo { get; set; }

    [Column("lusecusitm")]
    public short Lusecusitm { get; set; }

    [Column("lfrttax1")]
    public short Lfrttax1 { get; set; }

    [Column("lfrttax2")]
    public short Lfrttax2 { get; set; }

    [Column("lapplytax")]
    public short Lapplytax { get; set; }

    [Column("lprcinctax")]
    public short Lprcinctax { get; set; }

    [Column("lprtsord")]
    public short Lprtsord { get; set; }

    [Column("lprtlist")]
    public short Lprtlist { get; set; }

    [Column("lprtcod")]
    public short Lprtcod { get; set; }

    [Column("lprtlbl")]
    public short Lprtlbl { get; set; }

    [Column("lsavecard")]
    public short Lsavecard { get; set; }

    [Column("ndiscday")]
    public int Ndiscday { get; set; }

    [Column("ndueday")]
    public int Ndueday { get; set; }

    [Column("ntermdisc", TypeName = "numeric(6, 2)")]
    public decimal Ntermdisc { get; set; }

    [Column("ndiscrate", TypeName = "numeric(6, 2)")]
    public decimal Ndiscrate { get; set; }

    [Column("ntaxver", TypeName = "numeric(5, 0)")]
    public decimal Ntaxver { get; set; }

    [Column("nfrtaxver", TypeName = "numeric(5, 0)")]
    public decimal Nfrtaxver { get; set; }

    [Column("ntaxable1", TypeName = "numeric(18, 4)")]
    public decimal Ntaxable1 { get; set; }

    [Column("ntaxable2", TypeName = "numeric(18, 4)")]
    public decimal Ntaxable2 { get; set; }

    [Column("nsalesamt", TypeName = "numeric(18, 4)")]
    public decimal Nsalesamt { get; set; }

    [Column("ndiscamt", TypeName = "numeric(18, 4)")]
    public decimal Ndiscamt { get; set; }

    [Column("nfrtamt", TypeName = "numeric(18, 4)")]
    public decimal Nfrtamt { get; set; }

    [Column("ntaxamt1", TypeName = "numeric(18, 4)")]
    public decimal Ntaxamt1 { get; set; }

    [Column("ntaxamt2", TypeName = "numeric(18, 4)")]
    public decimal Ntaxamt2 { get; set; }

    [Column("ntaxamt3", TypeName = "numeric(18, 4)")]
    public decimal Ntaxamt3 { get; set; }

    [Column("nfrttax1", TypeName = "numeric(18, 4)")]
    public decimal Nfrttax1 { get; set; }

    [Column("nfrttax2", TypeName = "numeric(18, 4)")]
    public decimal Nfrttax2 { get; set; }

    [Column("nfrttax3", TypeName = "numeric(18, 4)")]
    public decimal Nfrttax3 { get; set; }

    [Column("nadjamt", TypeName = "numeric(18, 4)")]
    public decimal Nadjamt { get; set; }

    [Column("nadjusted", TypeName = "numeric(18, 4)")]
    public decimal Nadjusted { get; set; }

    [Column("nfrtamtchg", TypeName = "numeric(18, 4)")]
    public decimal Nfrtamtchg { get; set; }

    [Column("nftaxable1", TypeName = "numeric(18, 4)")]
    public decimal Nftaxable1 { get; set; }

    [Column("nftaxable2", TypeName = "numeric(18, 4)")]
    public decimal Nftaxable2 { get; set; }

    [Column("nfsalesamt", TypeName = "numeric(18, 4)")]
    public decimal Nfsalesamt { get; set; }

    [Column("nfdiscamt", TypeName = "numeric(18, 4)")]
    public decimal Nfdiscamt { get; set; }

    [Column("nffrtamt", TypeName = "numeric(18, 4)")]
    public decimal Nffrtamt { get; set; }

    [Column("nftaxamt1", TypeName = "numeric(18, 4)")]
    public decimal Nftaxamt1 { get; set; }

    [Column("nftaxamt2", TypeName = "numeric(18, 4)")]
    public decimal Nftaxamt2 { get; set; }

    [Column("nftaxamt3", TypeName = "numeric(18, 4)")]
    public decimal Nftaxamt3 { get; set; }

    [Column("nffrttax1", TypeName = "numeric(18, 4)")]
    public decimal Nffrttax1 { get; set; }

    [Column("nffrttax2", TypeName = "numeric(18, 4)")]
    public decimal Nffrttax2 { get; set; }

    [Column("nffrttax3", TypeName = "numeric(18, 4)")]
    public decimal Nffrttax3 { get; set; }

    [Column("nfadjamt", TypeName = "numeric(18, 4)")]
    public decimal Nfadjamt { get; set; }

    [Column("nfadjusted", TypeName = "numeric(18, 4)")]
    public decimal Nfadjusted { get; set; }

    [Column("nffrtamtch", TypeName = "numeric(18, 4)")]
    public decimal Nffrtamtch { get; set; }

    [Column("nweight", TypeName = "numeric(16, 2)")]
    public decimal Nweight { get; set; }

    [Column("nxchgrate", TypeName = "numeric(16, 6)")]
    public decimal Nxchgrate { get; set; }

    [Column("lsource")]
    public short? Lsource { get; set; }

    [Column("lautoinvk")]
    public short Lautoinvk { get; set; }

    [Column("ctranref")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctranref { get; set; } = null!;

    [Column("dexpire", TypeName = "datetime")]
    public DateTime? Dexpire { get; set; }

    [Column("dquote", TypeName = "datetime")]
    public DateTime? Dquote { get; set; }

    [Column("lcrhold")]
    public short Lcrhold { get; set; }

    [Column("lautoacpt")]
    public short Lautoacpt { get; set; }

    [Column("cbemail")]
    [StringLength(250)]
    [Unicode(false)]
    public string Cbemail { get; set; } = null!;

    [Column("csemail")]
    [StringLength(250)]
    [Unicode(false)]
    public string Csemail { get; set; } = null!;
}




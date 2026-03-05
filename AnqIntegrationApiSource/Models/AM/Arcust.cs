using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("arcust")]
public partial class Arcust
{
    [Key]
    [Column("ccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccustno { get; set; } = null!;

    [Column("ccompany")]
    [StringLength(40)]
    [Unicode(false)]
    public string Ccompany { get; set; } = null!;

    [Column("ccompany2")]
    [StringLength(40)]
    [Unicode(false)]
    public string Ccompany2 { get; set; } = null!;

    [Column("caddr1")]
    [StringLength(40)]
    [Unicode(false)]
    public string Caddr1 { get; set; } = null!;

    [Column("caddr2")]
    [StringLength(40)]
    [Unicode(false)]
    public string Caddr2 { get; set; } = null!;

    [Column("ccity")]
    [StringLength(30)]
    [Unicode(false)]
    public string Ccity { get; set; } = null!;

    [Column("cstate")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cstate { get; set; } = null!;

    [Column("czip")]
    [StringLength(10)]
    [Unicode(false)]
    public string Czip { get; set; } = null!;

    [Column("ccountry")]
    [StringLength(25)]
    [Unicode(false)]
    public string Ccountry { get; set; } = null!;

    [Column("cphone1")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cphone1 { get; set; } = null!;

    [Column("cphone2")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cphone2 { get; set; } = null!;

    [Column("cfax")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cfax { get; set; } = null!;

    [Column("cemail")]
    [StringLength(250)]
    [Unicode(false)]
    public string Cemail { get; set; } = null!;

    [Column("cwebsite")]
    [StringLength(250)]
    [Unicode(false)]
    public string Cwebsite { get; set; } = null!;

    [Column("cfname")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Cfname { get; set; }

    [Column("clname")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Clname { get; set; }

    [Column("cdear")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cdear { get; set; } = null!;

    [Column("ctitle")]
    [StringLength(30)]
    [Unicode(false)]
    public string Ctitle { get; set; } = null!;

    [Column("corderby")]
    [StringLength(30)]
    [Unicode(false)]
    public string Corderby { get; set; } = null!;

    [Column("cslpnno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cslpnno { get; set; } = null!;

    [Column("cstatus")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cstatus { get; set; } = null!;

    [Column("cclass")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cclass { get; set; } = null!;

    [Column("cindustry")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cindustry { get; set; } = null!;

    [Column("cterr")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cterr { get; set; } = null!;

    [Column("cwarehouse")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cwarehouse { get; set; } = null!;

    [Column("cpaycode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cpaycode { get; set; } = null!;

    [Column("cbilltono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbilltono { get; set; } = null!;

    [Column("cshiptono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cshiptono { get; set; } = null!;

    [Column("ctaxcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctaxcode { get; set; } = null!;

    [Column("crevncode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Crevncode { get; set; } = null!;

    [Column("ctaxfld1")]
    [StringLength(16)]
    [Unicode(false)]
    public string Ctaxfld1 { get; set; } = null!;

    [Column("ctaxfld2")]
    [StringLength(16)]
    [Unicode(false)]
    public string Ctaxfld2 { get; set; } = null!;

    [Column("ccurrcode")]
    [StringLength(3)]
    [Unicode(false)]
    public string Ccurrcode { get; set; } = null!;

    [Column("cprtstmt")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cprtstmt { get; set; } = null!;

    [Column("caracc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Caracc { get; set; }

    [Column("crcalactn")]
    [StringLength(35)]
    [Unicode(false)]
    public string Crcalactn { get; set; } = null!;

    [Column("clcalactn")]
    [StringLength(35)]
    [Unicode(false)]
    public string Clcalactn { get; set; } = null!;

    [Column("cpasswd")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cpasswd { get; set; } = null!;

    [Column("cpricecd")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cpricecd { get; set; } = null!;

    [Column("cpcustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cpcustno { get; set; } = null!;

    [Column("dcreate", TypeName = "datetime")]
    public DateTime Dcreate { get; set; }

    [Column("dytdstart", TypeName = "datetime")]
    public DateTime? Dytdstart { get; set; }

    [Column("tmodified", TypeName = "datetime")]
    public DateTime? Tmodified { get; set; }

    [Column("trecall", TypeName = "datetime")]
    public DateTime? Trecall { get; set; }

    [Column("tlcall", TypeName = "datetime")]
    public DateTime? Tlcall { get; set; }

    [Column("lprtstmt")]
    public short Lprtstmt { get; set; }

    [Column("lconstmt")]
    public short Lconstmt { get; set; }

    [Column("lfinchg")]
    public short Lfinchg { get; set; }

    [Column("liocust")]
    public short Liocust { get; set; }

    [Column("lusecusitm")]
    public short Lusecusitm { get; set; }

    [Column("luseitemno")]
    public short Luseitemno { get; set; }

    [Column("lusecusprc")]
    public short Lusecusprc { get; set; }

    [Column("lgeninvc")]
    public short Lgeninvc { get; set; }

    [Column("luselprice")]
    public short Luselprice { get; set; }

    [Column("lapplytax")]
    public short Lapplytax { get; set; }

    [Column("lprcinctax")]
    public short Lprcinctax { get; set; }

    [Column("lsavecard")]
    public short Lsavecard { get; set; }

    [Column("nexpdays")]
    public int Nexpdays { get; set; }

    [Column("navgdays")]
    public int? Navgdays { get; set; }

    [Column("ndiscrate", TypeName = "numeric(6, 2)")]
    public decimal Ndiscrate { get; set; }

    [Column("natdsamt", TypeName = "numeric(18, 4)")]
    public decimal Natdsamt { get; set; }

    [Column("nytdsamt", TypeName = "numeric(18, 4)")]
    public decimal Nytdsamt { get; set; }

    [Column("ncrlimit", TypeName = "numeric(18, 4)")]
    public decimal Ncrlimit { get; set; }

    [Column("nsoboamt", TypeName = "numeric(18, 4)")]
    public decimal Nsoboamt { get; set; }

    [Column("nopencr", TypeName = "numeric(18, 4)")]
    public decimal Nopencr { get; set; }

    [Column("nuishpamt", TypeName = "numeric(18, 4)")]
    public decimal Nuishpamt { get; set; }

    [Column("nbalance", TypeName = "numeric(18, 4)")]
    public decimal Nbalance { get; set; }

    [Column("mimptsord", TypeName = "text")]
    public string Mimptsord { get; set; } = null!;

    [Column("mimptstrs", TypeName = "text")]
    public string Mimptstrs { get; set; } = null!;

    [Column("mimptinvc", TypeName = "text")]
    public string Mimptinvc { get; set; } = null!;

    [Column("mimptitrs", TypeName = "text")]
    public string Mimptitrs { get; set; } = null!;

    [Column("crating")]
    [StringLength(10)]
    [Unicode(false)]
    public string Crating { get; set; } = null!;

    [Column("cshipvia")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cshipvia { get; set; } = null!;

    [Column("cdelvday")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cdelvday { get; set; } = null!;

    [Column("callocate")]
    [StringLength(15)]
    [Unicode(false)]
    public string Callocate { get; set; } = null!;

    [Column("cIdNo")]
    [StringLength(15)]
    [Unicode(false)]
    public string CIdNo { get; set; } = null!;

    [Column("cGender")]
    [StringLength(10)]
    [Unicode(false)]
    public string CGender { get; set; } = null!;

    [Column("cRace")]
    [StringLength(10)]
    [Unicode(false)]
    public string CRace { get; set; } = null!;

    [Column("cTmLdStat")]
    [StringLength(10)]
    [Unicode(false)]
    public string CTmLdStat { get; set; } = null!;

    [Column("dBirthday", TypeName = "datetime")]
    public DateTime DBirthday { get; set; }

    [Column("dBBDate", TypeName = "datetime")]
    public DateTime DBbdate { get; set; }

    [Column("dTLDate", TypeName = "datetime")]
    public DateTime DTldate { get; set; }

    [Column("lallowcmpgn")]
    public short Lallowcmpgn { get; set; }

    [Column("cbankno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbankno { get; set; } = null!;

    [Column("cbankname")]
    [StringLength(35)]
    [Unicode(false)]
    public string Cbankname { get; set; } = null!;

    [Column("cbranchno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbranchno { get; set; } = null!;

    [Column("cbrnchnme")]
    [StringLength(35)]
    [Unicode(false)]
    public string Cbrnchnme { get; set; } = null!;

    [Column("chldrname")]
    [StringLength(30)]
    [Unicode(false)]
    public string Chldrname { get; set; } = null!;

    [Column("caccttype")]
    [StringLength(1)]
    [Unicode(false)]
    public string Caccttype { get; set; } = null!;

    [Column("cbankacc")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cbankacc { get; set; } = null!;

    [Column("lemail")]
    public short Lemail { get; set; }

    [Column("lsms")]
    public short Lsms { get; set; }

    [Column("lpost")]
    public short Lpost { get; set; }

    [Column("lminorddsc")]
    public short Lminorddsc { get; set; }

    [Column("lmindevord")]
    public short Lmindevord { get; set; }

    [Column("csponsor")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csponsor { get; set; } = null!;

    [Column("lrgstrnpaid")]
    public short Lrgstrnpaid { get; set; }

    [Column("lhold")]
    public short Lhold { get; set; }

    [Column("cusername")]
    [StringLength(30)]
    [Unicode(false)]
    public string Cusername { get; set; } = null!;

    [Column("cregmethod")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cregmethod { get; set; } = null!;

    [Column("lpobox")]
    public short Lpobox { get; set; }

    [Column("lsuspended")]
    public short Lsuspended { get; set; }

    [Column("lrebate")]
    public short Lrebate { get; set; }

    [Column("lupdateportal")]
    public short Lupdateportal { get; set; }

    [Column("lexadmfee")]
    public short Lexadmfee { get; set; }

    [Column("cstatuscon")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Cstatuscon { get; set; }

    [Column("cstatusovr")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Cstatusovr { get; set; }

    [Column("cstatuslst")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Cstatuslst { get; set; }

    [Column("dstatuschg", TypeName = "datetime")]
    public DateTime? Dstatuschg { get; set; }

    [Column("dlastlet", TypeName = "datetime")]
    public DateTime? Dlastlet { get; set; }

    [Column("clastlet")]
    [StringLength(1)]
    [Unicode(false)]
    public string? Clastlet { get; set; }

    [Column("lrsa")]
    [StringLength(1)]
    [Unicode(false)]
    public string Lrsa { get; set; } = null!;

    [Column("cNation")]
    [StringLength(25)]
    [Unicode(false)]
    public string CNation { get; set; } = null!;

    [Column("cmarstat")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cmarstat { get; set; } = null!;

    [Column("cpfirstname")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cpfirstname { get; set; } = null!;

    [Column("cpname")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cpname { get; set; } = null!;

    [Column("cpcell")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cpcell { get; set; } = null!;

    [Column("cphone3")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cphone3 { get; set; } = null!;

    [Column("cpemail")]
    [StringLength(250)]
    [Unicode(false)]
    public string Cpemail { get; set; } = null!;

    [Column("cfacebook")]
    [StringLength(250)]
    [Unicode(false)]
    public string Cfacebook { get; set; } = null!;

    [Column("ctwitter")]
    [StringLength(250)]
    [Unicode(false)]
    public string Ctwitter { get; set; } = null!;

    [Column("cesponsor")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cesponsor { get; set; } = null!;

    [Column("lstarter")]
    public int Lstarter { get; set; }

    [Column("lpatitle")]
    public int Lpatitle { get; set; }

    [Column("ltitle")]
    public int Ltitle { get; set; }

    [Column("laccept")]
    public short Laccept { get; set; }

    [Column("dstarter", TypeName = "datetime")]
    public DateTime? Dstarter { get; set; }

    [Column("clanguage")]
    [StringLength(10)]
    [Unicode(false)]
    public string Clanguage { get; set; } = null!;

    [Column("daccept", TypeName = "datetime")]
    public DateTime? Daccept { get; set; }

    [Column("lshownote")]
    public short Lshownote { get; set; }

    [Column("ncrholdamt", TypeName = "numeric(18, 4)")]
    public decimal Ncrholdamt { get; set; }

    [Column("clinvno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Clinvno { get; set; } = null!;

    [Column("clrcptno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Clrcptno { get; set; } = null!;

    [Column("clinvcurr")]
    [StringLength(3)]
    [Unicode(false)]
    public string Clinvcurr { get; set; } = null!;

    [Column("clrcptcurr")]
    [StringLength(3)]
    [Unicode(false)]
    public string Clrcptcurr { get; set; } = null!;

    [Column("dlsales", TypeName = "datetime")]
    public DateTime? Dlsales { get; set; }

    [Column("dlrcpt", TypeName = "datetime")]
    public DateTime? Dlrcpt { get; set; }

    [Column("nlsalesamt", TypeName = "numeric(18, 4)")]
    public decimal Nlsalesamt { get; set; }

    [Column("nlrcptamt", TypeName = "numeric(18, 4)")]
    public decimal Nlrcptamt { get; set; }

    [Column("nsqamt", TypeName = "numeric(18, 4)")]
    public decimal Nsqamt { get; set; }

    [Column("dtempcrvld", TypeName = "datetime")]
    public DateTime? Dtempcrvld { get; set; }

    [Column("ntempcrinc", TypeName = "numeric(18, 4)")]
    public decimal Ntempcrinc { get; set; }

    [Column("mcrhistory", TypeName = "text")]
    public string Mcrhistory { get; set; } = null!;

    [Column("nadvbillpmt", TypeName = "numeric(18, 4)")]
    public decimal Nadvbillpmt { get; set; }

    [Column("cbankacct")]
    [StringLength(32)]
    public string? Cbankacct { get; set; }

    [Column("cbnkroute")]
    [StringLength(9)]
    [Unicode(false)]
    public string Cbnkroute { get; set; } = null!;

    [Column("cprenote")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cprenote { get; set; } = null!;

    [Column("cssn")]
    [StringLength(50)]
    [Unicode(false)]
    public string Cssn { get; set; } = null!;

    [Column("dprenote", TypeName = "datetime")]
    public DateTime? Dprenote { get; set; }

    [Column("lepayment")]
    public short Lepayment { get; set; }

    [Column("dytdrecalc", TypeName = "datetime")]
    public DateTime? Dytdrecalc { get; set; }

    [Column("cfob")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cfob { get; set; } = null!;
}




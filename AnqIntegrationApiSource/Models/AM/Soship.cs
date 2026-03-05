using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("soship")]
public partial class Soship
{
    [Key]
    [Column("cshipno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cshipno { get; set; } = null!;

    [Column("csono")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csono { get; set; } = null!;

    [Column("ccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccustno { get; set; } = null!;

    [Column("cwarehouse")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cwarehouse { get; set; } = null!;

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

    [Column("caracc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Caracc { get; set; }

    [Column("ccommiss")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccommiss { get; set; } = null!;

    [Column("csource")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csource { get; set; } = null!;

    [Column("dship", TypeName = "datetime")]
    public DateTime Dship { get; set; }

    [Column("dorder", TypeName = "datetime")]
    public DateTime? Dorder { get; set; }

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

    [Column("lprtslip")]
    public short Lprtslip { get; set; }

    [Column("lprtlbl")]
    public short Lprtlbl { get; set; }

    [Column("ntaxver", TypeName = "numeric(5, 0)")]
    public decimal Ntaxver { get; set; }

    [Column("nfrtaxver", TypeName = "numeric(5, 0)")]
    public decimal Nfrtaxver { get; set; }

    [Column("nfrtamt", TypeName = "numeric(18, 4)")]
    public decimal Nfrtamt { get; set; }

    [Column("nadjamt", TypeName = "numeric(18, 4)")]
    public decimal Nadjamt { get; set; }

    [Column("nfrttax1", TypeName = "numeric(18, 4)")]
    public decimal Nfrttax1 { get; set; }

    [Column("nfrttax2", TypeName = "numeric(18, 4)")]
    public decimal Nfrttax2 { get; set; }

    [Column("nfrttax3", TypeName = "numeric(18, 4)")]
    public decimal Nfrttax3 { get; set; }

    [Column("nffrtamt", TypeName = "numeric(18, 4)")]
    public decimal Nffrtamt { get; set; }

    [Column("nfadjamt", TypeName = "numeric(18, 4)")]
    public decimal Nfadjamt { get; set; }

    [Column("nffrttax1", TypeName = "numeric(18, 4)")]
    public decimal Nffrttax1 { get; set; }

    [Column("nffrttax2", TypeName = "numeric(18, 4)")]
    public decimal Nffrttax2 { get; set; }

    [Column("nffrttax3", TypeName = "numeric(18, 4)")]
    public decimal Nffrttax3 { get; set; }

    [Column("nweight", TypeName = "numeric(16, 2)")]
    public decimal Nweight { get; set; }

    [Column("nxchgrate", TypeName = "numeric(16, 6)")]
    public decimal Nxchgrate { get; set; }

    [Column("cparcel", TypeName = "text")]
    public string Cparcel { get; set; } = null!;

    [Column("cSstatus")]
    [StringLength(3)]
    [Unicode(false)]
    public string CSstatus { get; set; } = null!;

    [Column("clastshipstatus")]
    [StringLength(3)]
    [Unicode(false)]
    public string Clastshipstatus { get; set; } = null!;

    [Column("cCourierinv")]
    [StringLength(20)]
    public string CCourierinv { get; set; } = null!;

    [Column("cWaybillno")]
    [StringLength(12)]
    public string CWaybillno { get; set; } = null!;

    [Column("nshipcost", TypeName = "numeric(15, 4)")]
    public decimal Nshipcost { get; set; }

    [Column("cbemail")]
    [StringLength(250)]
    [Unicode(false)]
    public string Cbemail { get; set; } = null!;

    [Column("csemail")]
    [StringLength(250)]
    [Unicode(false)]
    public string Csemail { get; set; } = null!;
}




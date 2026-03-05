using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("icitem")]
public partial class Icitem
{
    [Key]
    [Column("citemno")]
    [StringLength(20)]
    [Unicode(false)]
    public string Citemno { get; set; } = null!;

    [Column("cdescript")]
    [StringLength(54)]
    [Unicode(false)]
    public string Cdescript { get; set; } = null!;

    [Column("cfdescript")]
    [StringLength(54)]
    [Unicode(false)]
    public string Cfdescript { get; set; } = null!;

    [Column("ctype")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctype { get; set; } = null!;

    [Column("cspectype1")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cspectype1 { get; set; } = null!;

    [Column("cspectype2")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cspectype2 { get; set; } = null!;

    [Column("cbarcode1")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cbarcode1 { get; set; } = null!;

    [Column("cbarcode2")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cbarcode2 { get; set; } = null!;

    [Column("cstatus")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cstatus { get; set; } = null!;

    [Column("cmeasure")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cmeasure { get; set; } = null!;

    [Column("csmeasure")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csmeasure { get; set; } = null!;

    [Column("cpmeasure")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cpmeasure { get; set; } = null!;

    [Column("cclass")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cclass { get; set; } = null!;

    [Column("cprodline")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cprodline { get; set; } = null!;

    [Column("ccommiss")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccommiss { get; set; } = null!;

    [Column("cvendno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cvendno { get; set; } = null!;

    [Column("cminptype")]
    [StringLength(2)]
    [Unicode(false)]
    public string Cminptype { get; set; } = null!;

    [Column("cbuyer")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cbuyer { get; set; } = null!;

    [Column("ctaxcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctaxcode { get; set; } = null!;

    [Column("dcreate", TypeName = "datetime")]
    public DateTime Dcreate { get; set; }

    [Column("dlastsale", TypeName = "datetime")]
    public DateTime? Dlastsale { get; set; }

    [Column("dlastfnh", TypeName = "datetime")]
    public DateTime? Dlastfnh { get; set; }

    [Column("dspstart", TypeName = "datetime")]
    public DateTime? Dspstart { get; set; }

    [Column("dspend", TypeName = "datetime")]
    public DateTime? Dspend { get; set; }

    [Column("tmodified", TypeName = "datetime")]
    public DateTime? Tmodified { get; set; }

    [Column("lusespec")]
    public short Lusespec { get; set; }

    [Column("laritem")]
    public short Laritem { get; set; }

    [Column("lpoitem")]
    public short Lpoitem { get; set; }

    [Column("lmiitem")]
    public short Lmiitem { get; set; }

    [Column("lioitem")]
    public short Lioitem { get; set; }

    [Column("lkititem")]
    public short Lkititem { get; set; }

    [Column("lusekitno")]
    public short Lusekitno { get; set; }

    [Column("llot")]
    public short Llot { get; set; }

    [Column("lsubitem")]
    public short Lsubitem { get; set; }

    [Column("lcomplst")]
    public short Lcomplst { get; set; }

    [Column("lmlprice")]
    public short Lmlprice { get; set; }

    [Column("lchkonhand")]
    public short Lchkonhand { get; set; }

    [Column("lupdonhand")]
    public short Lupdonhand { get; set; }

    [Column("ltaxable1")]
    public short Ltaxable1 { get; set; }

    [Column("ltaxable2")]
    public short Ltaxable2 { get; set; }

    [Column("lallownupd")]
    public short Lallownupd { get; set; }

    [Column("lallowneg")]
    public short Lallowneg { get; set; }

    [Column("lnegprice")]
    public short Lnegprice { get; set; }

    [Column("lowdesc")]
    public short Lowdesc { get; set; }

    [Column("lowprice")]
    public short Lowprice { get; set; }

    [Column("lowdisc")]
    public short Lowdisc { get; set; }

    [Column("lowtax")]
    public short Lowtax { get; set; }

    [Column("lowweight")]
    public short Lowweight { get; set; }

    [Column("lowrevncd")]
    public short Lowrevncd { get; set; }

    [Column("lowcomp")]
    public short Lowcomp { get; set; }

    [Column("lprtsn")]
    public short Lprtsn { get; set; }

    [Column("lprtlotno")]
    public short Lprtlotno { get; set; }

    [Column("lowivrmk")]
    public short Lowivrmk { get; set; }

    [Column("lptivrmk")]
    public short Lptivrmk { get; set; }

    [Column("lowsormk")]
    public short Lowsormk { get; set; }

    [Column("lptsormk")]
    public short Lptsormk { get; set; }

    [Column("lowpormk")]
    public short Lowpormk { get; set; }

    [Column("lptpormk")]
    public short Lptpormk { get; set; }

    [Column("lowmirmk")]
    public short Lowmirmk { get; set; }

    [Column("lptmirmk")]
    public short Lptmirmk { get; set; }

    [Column("lowcoms")]
    public short Lowcoms { get; set; }

    [Column("ldiscard")]
    public short Ldiscard { get; set; }

    [Column("lrepair")]
    public short Lrepair { get; set; }

    [Column("lowrarmk")]
    public short Lowrarmk { get; set; }

    [Column("lptrarmk")]
    public short Lptrarmk { get; set; }

    [Column("llifetime")]
    public short Llifetime { get; set; }

    [Column("lprebkit")]
    public short Lprebkit { get; set; }

    [Column("lupsitem")]
    public short Lupsitem { get; set; }

    [Column("lupsubspec")]
    public short Lupsubspec { get; set; }

    [Column("ncosttype")]
    public int Ncosttype { get; set; }

    [Column("nminprice", TypeName = "numeric(16, 4)")]
    public decimal Nminprice { get; set; }

    [Column("nqtydec")]
    public int Nqtydec { get; set; }

    [Column("ndiscrate", TypeName = "numeric(6, 2)")]
    public decimal Ndiscrate { get; set; }

    [Column("nweight", TypeName = "numeric(16, 2)")]
    public decimal Nweight { get; set; }

    [Column("nstdcost", TypeName = "numeric(16, 4)")]
    public decimal Nstdcost { get; set; }

    [Column("nrtrncost", TypeName = "numeric(16, 4)")]
    public decimal Nrtrncost { get; set; }

    [Column("nlfnhcost", TypeName = "numeric(16, 4)")]
    public decimal Nlfnhcost { get; set; }

    [Column("nprice", TypeName = "numeric(16, 4)")]
    public decimal Nprice { get; set; }

    [Column("nprcinctx", TypeName = "numeric(16, 4)")]
    public decimal Nprcinctx { get; set; }

    [Column("nspprice", TypeName = "numeric(16, 4)")]
    public decimal Nspprice { get; set; }

    [Column("nspprinctx", TypeName = "numeric(16, 4)")]
    public decimal Nspprinctx { get; set; }

    [Column("nlsalprice", TypeName = "numeric(16, 4)")]
    public decimal Nlsalprice { get; set; }

    [Column("nlsprinctx", TypeName = "numeric(16, 4)")]
    public decimal Nlsprinctx { get; set; }

    [Column("nrstkpct", TypeName = "numeric(6, 2)")]
    public decimal Nrstkpct { get; set; }

    [Column("nminrstk", TypeName = "numeric(18, 4)")]
    public decimal Nminrstk { get; set; }

    [Column("nmrsinctx", TypeName = "numeric(18, 4)")]
    public decimal Nmrsinctx { get; set; }

    [Column("nrepprice", TypeName = "numeric(16, 4)")]
    public decimal Nrepprice { get; set; }

    [Column("nrpprinctx", TypeName = "numeric(16, 4)")]
    public decimal Nrpprinctx { get; set; }

    [Column("cvernum")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cvernum { get; set; } = null!;

    [Column("dversion", TypeName = "datetime")]
    public DateTime? Dversion { get; set; }

    [Column("cstkcycle")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cstkcycle { get; set; } = null!;

    [Column("ccurstat")]
    [StringLength(1)]
    [Unicode(false)]
    public string Ccurstat { get; set; } = null!;

    [Column("cprevstat")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cprevstat { get; set; } = null!;

    [Column("dcurstat", TypeName = "datetime")]
    public DateTime? Dcurstat { get; set; }

    [Column("dprevstat", TypeName = "datetime")]
    public DateTime? Dprevstat { get; set; }

    [Column("csponcat")]
    [StringLength(1)]
    [Unicode(false)]
    public string Csponcat { get; set; } = null!;

    [Column("lmlm")]
    public short Lmlm { get; set; }

    [Column("ldscntble")]
    public short Ldscntble { get; set; }

    [Column("it_tmodified", TypeName = "datetime")]
    public DateTime? ItTmodified { get; set; }

    [Column("it_modified_yn")]
    [StringLength(1)]
    [Unicode(false)]
    public string? ItModifiedYn { get; set; }

    [Column("ldrp")]
    public short Ldrp { get; set; }

    [Column("cusername")]
    [StringLength(30)]
    [Unicode(false)]
    public string Cusername { get; set; } = null!;

    [Column("ctype1")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctype1 { get; set; } = null!;

    [Column("ctype2")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctype2 { get; set; } = null!;

    [Column("ctype3")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctype3 { get; set; } = null!;

    [Column("ldisc")]
    public short Ldisc { get; set; }

    [Column("cdfsponcat")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cdfsponcat { get; set; } = null!;

    [Column("cnxsponcat")]
    [StringLength(1)]
    [Unicode(false)]
    public string Cnxsponcat { get; set; } = null!;

    [Column("deffective", TypeName = "datetime")]
    public DateTime Deffective { get; set; }

    [Column("csubcat")]
    [StringLength(10)]
    [Unicode(false)]
    public string Csubcat { get; set; } = null!;

    [Column("nnwprice", TypeName = "numeric(18, 4)")]
    public decimal Nnwprice { get; set; }

    [Column("nnwprinctx", TypeName = "numeric(18, 4)")]
    public decimal Nnwprinctx { get; set; }

    [Column("dnwstart", TypeName = "datetime")]
    public DateTime? Dnwstart { get; set; }

    [Column("dnwend", TypeName = "datetime")]
    public DateTime? Dnwend { get; set; }

    [Column("duntstart", TypeName = "datetime")]
    public DateTime? Duntstart { get; set; }

    [Column("duntend", TypeName = "datetime")]
    public DateTime? Duntend { get; set; }

    [Column("lregitem")]
    public short Lregitem { get; set; }

    [Column("linvksales")]
    public short Linvksales { get; set; }

    [Column("lrecoup")]
    public short Lrecoup { get; set; }

    [Column("lSalesaid")]
    public short LSalesaid { get; set; }

    [Column("nBoxCount")]
    public short NBoxCount { get; set; }

    [Column("nQv", TypeName = "numeric(12, 2)")]
    public decimal? NQv { get; set; }

    [Column("nsp", TypeName = "numeric(12, 2)")]
    public decimal? Nsp { get; set; }

    [Column("lptarpsrmk")]
    public short Lptarpsrmk { get; set; }

    [Column("lptsoplrmk")]
    public short Lptsoplrmk { get; set; }

    [Column("lptsopsrmk")]
    public short Lptsopsrmk { get; set; }

    [Column("lptraplrmk")]
    public short Lptraplrmk { get; set; }

    [Column("camrtmethd")]
    [StringLength(1)]
    [Unicode(false)]
    public string Camrtmethd { get; set; } = null!;

    [Column("camrtrecur")]
    [StringLength(1)]
    [Unicode(false)]
    public string Camrtrecur { get; set; } = null!;

    [Column("cccostacc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Cccostacc { get; set; }

    [Column("ccobliacc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Ccobliacc { get; set; }

    [Column("ccdiscacc")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Ccdiscacc { get; set; }

    [Column("lamortize")]
    public short Lamortize { get; set; }

    [Column("namrtcycle")]
    public int Namrtcycle { get; set; }

    [Column("lusestdcst")]
    public short Lusestdcst { get; set; }

    [Column("lcusxitm")]
    public int Lcusxitm { get; set; }
}




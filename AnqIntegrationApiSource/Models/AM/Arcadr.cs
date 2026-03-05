using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[PrimaryKey("Ccustno", "Caddrno")]
[Table("arcadr")]
public partial class Arcadr
{
    [Key]
    [Column("ccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ccustno { get; set; } = null!;

    [Key]
    [Column("caddrno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Caddrno { get; set; } = null!;

    [Column("ccompany")]
    [StringLength(40)]
    [Unicode(false)]
    public string Ccompany { get; set; } = null!;

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

    [Column("cphone")]
    [StringLength(20)]
    [Unicode(false)]
    public string Cphone { get; set; } = null!;

    [Column("ccontact")]
    [StringLength(30)]
    [Unicode(false)]
    public string Ccontact { get; set; } = null!;

    [Column("ctaxcode")]
    [StringLength(10)]
    [Unicode(false)]
    public string Ctaxcode { get; set; } = null!;

    [Column("clevel")]
    [StringLength(1)]
    [Unicode(false)]
    public string Clevel { get; set; } = null!;

    [Column("caddrnoSC")]
    [StringLength(10)]
    [Unicode(false)]
    public string CaddrnoSc { get; set; } = null!;

    [Column("caddrstat")]
    [StringLength(10)]
    [Unicode(false)]
    public string Caddrstat { get; set; } = null!;

    [Column("cdrop")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cdrop { get; set; } = null!;

    [Column("djoined", TypeName = "datetime")]
    public DateTime? Djoined { get; set; }

    [Column("ddrop", TypeName = "datetime")]
    public DateTime? Ddrop { get; set; }

    [Column("cStar")]
    [StringLength(10)]
    [Unicode(false)]
    public string CStar { get; set; } = null!;

    [Column("ldiscount")]
    public short Ldiscount { get; set; }

    [Column("it_tmodified", TypeName = "datetime")]
    public DateTime? ItTmodified { get; set; }

    [Column("it_modified_yn")]
    [StringLength(1)]
    [Unicode(false)]
    public string? ItModifiedYn { get; set; }

    [Column("lpobox")]
    public short Lpobox { get; set; }

    [Column("cemail")]
    [StringLength(250)]
    [Unicode(false)]
    public string Cemail { get; set; } = null!;

    [Column("cshipvia")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cshipvia { get; set; } = null!;

    [Column("cfob")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cfob { get; set; } = null!;
}




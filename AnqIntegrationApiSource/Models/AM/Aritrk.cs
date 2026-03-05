using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.AM;

[Table("aritrk")]
public partial class Aritrk
{
    [Key]
    [Column("cuid")]
    [StringLength(15)]
    [Unicode(false)]
    public string Cuid { get; set; } = null!;

    [Column("cinvno")]
    [StringLength(10)]
    [Unicode(false)]
    public string Cinvno { get; set; } = null!;

    [Column("mremark", TypeName = "text")]
    public string Mremark { get; set; } = null!;
}




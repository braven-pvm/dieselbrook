using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Lookups")]
public partial class AnqLookup
{
    [Key]
    public int Id { get; set; }

    [Column("ctype")]
    [StringLength(12)]
    public string Ctype { get; set; } = null!;

    [Column("code")]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Column("description")]
    [StringLength(35)]
    public string Description { get; set; } = null!;

    public bool Iactive { get; set; }

    public int StoreId { get; set; }
}

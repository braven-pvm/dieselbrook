using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_ManufacturerIntegration")]
public partial class AnqManufacturerIntegration
{
    [Key]
    public int Id { get; set; }

    public int ManufacturerId { get; set; }

    [StringLength(20)]
    public string IntegrationField { get; set; } = null!;

    [StringLength(40)]
    public string IntegrationValue { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_CategoryIntegration")]
public partial class AnqCategoryIntegration
{
    [Key]
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [StringLength(20)]
    public string IntegrationField { get; set; } = null!;

    [StringLength(40)]
    public string IntegrationValue { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("AnqCategoryIntegrations")]
    public virtual Category Category { get; set; } = null!;
}

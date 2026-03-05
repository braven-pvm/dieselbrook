using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("CustomerRole")]
public partial class CustomerRole
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? SystemName { get; set; }

    public bool FreeShipping { get; set; }

    public bool TaxExempt { get; set; }

    public bool Active { get; set; }

    public bool IsSystemRole { get; set; }

    public bool EnablePasswordLifetime { get; set; }

    public bool OverrideTaxDisplayType { get; set; }

    public int DefaultTaxDisplayTypeId { get; set; }

    public int PurchasedWithProductId { get; set; }

    [ForeignKey("CustomerRoleId")]
    [InverseProperty("CustomerRoles")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

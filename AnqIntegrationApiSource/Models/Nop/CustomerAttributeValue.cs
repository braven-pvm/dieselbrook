using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("CustomerAttributeValue")]
public partial class CustomerAttributeValue
{
    [Key]
    public int Id { get; set; }

    [StringLength(400)]
    public string Name { get; set; } = null!;

    public int CustomerAttributeId { get; set; }

    public bool IsPreSelected { get; set; }

    public int DisplayOrder { get; set; }

    [ForeignKey("CustomerAttributeId")]
    [InverseProperty("CustomerAttributeValues")]
    public virtual CustomerAttribute CustomerAttribute { get; set; } = null!;
}

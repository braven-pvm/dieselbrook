using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("CustomerAttribute")]
public partial class CustomerAttribute
{
    [Key]
    public int Id { get; set; }

    [StringLength(400)]
    public string Name { get; set; } = null!;

    public bool IsRequired { get; set; }

    public int AttributeControlTypeId { get; set; }

    public int DisplayOrder { get; set; }

    [InverseProperty("CustomerAttribute")]
    public virtual ICollection<CustomerAttributeValue> CustomerAttributeValues { get; set; } = new List<CustomerAttributeValue>();
}

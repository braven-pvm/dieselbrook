using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[PrimaryKey("AddressId", "CustomerId")]
public partial class CustomerAddress
{
    [Key]
    [Column("Address_Id")]
    public int AddressId { get; set; }

    [Key]
    [Column("Customer_Id")]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerAddresses")]
    public virtual Customer Customer { get; set; } = null!;
}

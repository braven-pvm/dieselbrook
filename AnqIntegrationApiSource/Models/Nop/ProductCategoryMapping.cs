using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("Product_Category_Mapping")]
public partial class ProductCategoryMapping
{
    [Key]
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public int ProductId { get; set; }

    public bool IsFeaturedProduct { get; set; }

    public int DisplayOrder { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("ProductCategoryMappings")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductCategoryMappings")]
    public virtual Product Product { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_AwardShoppingCartItem")]
public partial class AnqAwardShoppingCartItem
{
    [Key]
    public int Id { get; set; }

    public int AwardId { get; set; }

    public int ShoppingCartItemId { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int StoreId { get; set; }

    public int Quantity { get; set; }
}

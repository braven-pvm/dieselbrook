using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_GiftsTaken")]
public partial class AnqGiftsTaken
{
    [Key]
    public int Id { get; set; }

    public int GiftId { get; set; }

    public int CustomerId { get; set; }

    public int OrderItemId { get; set; }

    public int Qty { get; set; }
}

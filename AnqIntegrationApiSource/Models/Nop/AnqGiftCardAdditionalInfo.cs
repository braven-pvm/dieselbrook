using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_GiftCardAdditionalInfo")]
public partial class AnqGiftCardAdditionalInfo
{
    [Key]
    public int Id { get; set; }

    public int GiftCardId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;
}

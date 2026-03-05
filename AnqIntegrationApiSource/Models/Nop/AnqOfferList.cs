using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_OfferList")]
public partial class AnqOfferList
{
    [Key]
    public int Id { get; set; }

    [StringLength(1)]
    public string? ListType { get; set; }

    public int? OfferId { get; set; }

    public int? ProductId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? CitemNo { get; set; }
}

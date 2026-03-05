using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ProductReview")]
public partial class ProductReview
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int StoreId { get; set; }

    public bool IsApproved { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public string ReviewText { get; set; } = null!;

    public string? ReplyText { get; set; }

    public bool CustomerNotifiedOfReply { get; set; }

    public int Rating { get; set; }

    public int HelpfulYesTotal { get; set; }

    public int HelpfulNoTotal { get; set; }

    [Precision(6)]
    public DateTime CreatedOnUtc { get; set; }

    // Optional navigation
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}

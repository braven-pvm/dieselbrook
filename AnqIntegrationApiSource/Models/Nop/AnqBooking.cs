using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Booking")]
public partial class AnqBooking
{
    [Key]
    public int Id { get; set; }

    [Column("EventID")]
    public int EventId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("ConsultantCustomerID")]
    public int? ConsultantCustomerId { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(10)]
    public string? Status { get; set; }

    public DateTime? DateBooked { get; set; }

    [StringLength(3)]
    public string? Attended { get; set; }

    [Column("OrderID")]
    public int? OrderId { get; set; }

    [Column("cSono")]
    [StringLength(10)]
    public string? CSono { get; set; }

    [Column("cInvno")]
    [StringLength(10)]
    public string? CInvno { get; set; }

    public bool? IsPrimaryRegistrant { get; set; }

    [Column("dlastupd")]
    public DateTime? Dlastupd { get; set; }

    [Column("IEmail")]
    public bool? Iemail { get; set; }
}

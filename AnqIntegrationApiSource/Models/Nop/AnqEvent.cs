using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("ANQ_Events")]
public partial class AnqEvent
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime StartDateTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndDateTime { get; set; }

    [StringLength(50)]
    public string? LocationName { get; set; }

    [StringLength(50)]
    public string? LocationAddress1 { get; set; }

    [StringLength(50)]
    public string? LocationAddress2 { get; set; }

    [StringLength(50)]
    public string? LocationCity { get; set; }

    [StringLength(50)]
    public string? LocationLocation { get; set; }

    [StringLength(50)]
    public string? LocationPostalCode { get; set; }

    [StringLength(50)]
    public string? LocationCountry { get; set; }

    [StringLength(50)]
    public string? ContactName { get; set; }

    [StringLength(50)]
    public string? ContactEmail { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [StringLength(255)]
    public string? ShortDescription { get; set; }

    [StringLength(20)]
    public string? TicketCode { get; set; }

    public bool Bookingopen { get; set; }

    [Column("IActive")]
    public bool Iactive { get; set; }

    [Column("dlastupd", TypeName = "datetime")]
    public DateTime? Dlastupd { get; set; }

    [StringLength(40)]
    public string? ZoomCode { get; set; }

    public bool? IsOnline { get; set; }

    public bool Published { get; set; }

    [Column("isField")]
    public bool? IsField { get; set; }

    [Column("isOptIn")]
    public bool? IsOptIn { get; set; }

    public int? CloseDays { get; set; }

    public int? BookingOpenDays { get; set; }

    [Column("HOAprovalDays")]
    public int? HoaprovalDays { get; set; }

    [Column("NOTIFICATIONDays")]
    public int? Notificationdays { get; set; }

    public int? LoadItemsDays { get; set; }

    public int? NotOrderedDays { get; set; }

    [Column("ProductID")]
    public int? ProductId { get; set; }
}

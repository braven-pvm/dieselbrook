using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;


[Table("ANQ_NewRegistrations")]
public partial class AnqNewRegistration
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("csponsor")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Csponsor { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOnUtc { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedOnUtc { get; set; }

    [Column("ccustno")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Ccustno { get; set; }

    [Column("cLname")]
    [StringLength(30)]
    [Unicode(false)]
    public string? CLname { get; set; }

    [Column("cFname")]
    [StringLength(30)]
    [Unicode(false)]
    public string? CFname { get; set; }

    [Column("cCompany")]
    [StringLength(40)]
    [Unicode(false)]
    public string? CCompany { get; set; }

    [Column("cTitle")]
    [StringLength(30)]
    [Unicode(false)]
    public string? CTitle { get; set; }

    [Column("cEmail")]
    [StringLength(250)]
    [Unicode(false)]
    public string? CEmail { get; set; }

    [Column("cPhone1")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CPhone1 { get; set; }

    [Column("cPhone3")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CPhone3 { get; set; }

    [Column("cPhone2")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CPhone2 { get; set; }

    [Column("cFax")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CFax { get; set; }

    [Column("cZip")]
    [StringLength(10)]
    [Unicode(false)]
    public string? CZip { get; set; }

    [Column("ccountry")]
    [StringLength(25)]
    [Unicode(false)]
    public string? Ccountry { get; set; }

    [Column("latitude", TypeName = "numeric(10, 4)")]
    public decimal? Latitude { get; set; }

    [Column("longitude", TypeName = "numeric(10, 4)")]
    public decimal? Longitude { get; set; }

    [Column("laccept")]
    public short Laccept { get; set; }

    [Column("daccept", TypeName = "datetime")]
    public DateTime? Daccept { get; set; }

    [Column("besttocall")]
    [StringLength(20)]
    public string? Besttocall { get; set; }

    [Column("hearabout")]
    [StringLength(20)]
    public string? Hearabout { get; set; }

    [Column("interests")]
    [StringLength(20)]
    public string? Interests { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Referredby { get; set; }

    [Column("SMSactive")]
    public bool? Smsactive { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? CreatedBy { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? LastUser { get; set; }

    [Column("IPAddress")]
    [StringLength(20)]
    public string? Ipaddress { get; set; }

    public string? Browser { get; set; }

    [StringLength(36)]
    [Unicode(false)]
    public string? ActivateLink { get; set; }
}

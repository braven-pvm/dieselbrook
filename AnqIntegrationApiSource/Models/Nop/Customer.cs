using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("Customer")]
public partial class Customer
{
    [Key]
    public int Id { get; set; }

    [StringLength(1000)]
    public string? Username { get; set; }

    [StringLength(1000)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? EmailToRevalidate { get; set; }

    [StringLength(1000)]
    public string? FirstName { get; set; }

    [StringLength(1000)]
    public string? LastName { get; set; }

    [StringLength(1000)]
    public string? Gender { get; set; }

    [StringLength(1000)]
    public string? Company { get; set; }

    [StringLength(1000)]
    public string? StreetAddress { get; set; }

    [StringLength(1000)]
    public string? StreetAddress2 { get; set; }

    [StringLength(1000)]
    public string? ZipPostalCode { get; set; }

    [StringLength(1000)]
    public string? City { get; set; }

    [StringLength(1000)]
    public string? County { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(1000)]
    public string? Fax { get; set; }

    [StringLength(1000)]
    public string? VatNumber { get; set; }

    [StringLength(1000)]
    public string? TimeZoneId { get; set; }

    [Column("CustomCustomerAttributesXML")]
    public string? CustomCustomerAttributesXml { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(400)]
    public string? SystemName { get; set; }

    public int? CurrencyId { get; set; }

    public int? LanguageId { get; set; }

    [Column("BillingAddress_Id")]
    public int? BillingAddressId { get; set; }

    [Column("ShippingAddress_Id")]
    public int? ShippingAddressId { get; set; }

    public Guid CustomerGuid { get; set; }

    public int CountryId { get; set; }

    public int StateProvinceId { get; set; }

    public int VatNumberStatusId { get; set; }

    public int? TaxDisplayTypeId { get; set; }

    public string? AdminComment { get; set; }

    public bool IsTaxExempt { get; set; }

    public int AffiliateId { get; set; }

    public int VendorId { get; set; }

    public bool HasShoppingCartItems { get; set; }

    public bool RequireReLogin { get; set; }

    public int FailedLoginAttempts { get; set; }

    [Precision(6)]
    public DateTime? CannotLoginUntilDateUtc { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public bool IsSystemAccount { get; set; }

    public string? LastIpAddress { get; set; }

    [Precision(6)]
    public DateTime CreatedOnUtc { get; set; }

    [Precision(6)]
    public DateTime? LastLoginDateUtc { get; set; }

    [Precision(6)]
    public DateTime LastActivityDateUtc { get; set; }

    public int RegisteredInStoreId { get; set; }

   
    public virtual ICollection<AnqDiscountAppliedToCustomer> AnqDiscountAppliedToCustomers { get; set; } = new List<AnqDiscountAppliedToCustomer>();

    public virtual ICollection<AnqDiscountAppliedToCustomersArchive> AnqDiscountAppliedToCustomersArchives { get; set; } = new List<AnqDiscountAppliedToCustomersArchive>();


    public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [ForeignKey("CustomerId")]

    public virtual ICollection<CustomerRole> CustomerRoles { get; set; } = new List<CustomerRole>();
}

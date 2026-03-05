using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Models.Nop;

[Table("Order")]
public partial class Order
{
    [Key]
    public int Id { get; set; }

    public string CustomOrderNumber { get; set; } = null!;

    public int BillingAddressId { get; set; }

    public int CustomerId { get; set; }

    public int? PickupAddressId { get; set; }

    public int? ShippingAddressId { get; set; }

    public Guid OrderGuid { get; set; }

    public int StoreId { get; set; }

    public bool PickupInStore { get; set; }

    public int OrderStatusId { get; set; }

    public int ShippingStatusId { get; set; }

    public int PaymentStatusId { get; set; }

    public string? PaymentMethodSystemName { get; set; }

    public string? CustomerCurrencyCode { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal CurrencyRate { get; set; }

    public int CustomerTaxDisplayTypeId { get; set; }

    public string? VatNumber { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderSubtotalInclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderSubtotalExclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderSubTotalDiscountInclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderSubTotalDiscountExclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderShippingInclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderShippingExclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal PaymentMethodAdditionalFeeInclTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal PaymentMethodAdditionalFeeExclTax { get; set; }

    public string? TaxRates { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderTax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderDiscount { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal OrderTotal { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal RefundedAmount { get; set; }

    public int? RewardPointsHistoryEntryId { get; set; }

    public string? CheckoutAttributeDescription { get; set; }

    public string? CheckoutAttributesXml { get; set; }

    public int CustomerLanguageId { get; set; }

    public int AffiliateId { get; set; }

    public string? CustomerIp { get; set; }

    public bool AllowStoringCreditCardNumber { get; set; }

    public string? CardType { get; set; }

    public string? CardName { get; set; }

    public string? CardNumber { get; set; }

    public string? MaskedCreditCardNumber { get; set; }

    public string? CardCvv2 { get; set; }

    public string? CardExpirationMonth { get; set; }

    public string? CardExpirationYear { get; set; }

    public string? AuthorizationTransactionId { get; set; }

    public string? AuthorizationTransactionCode { get; set; }

    public string? AuthorizationTransactionResult { get; set; }

    public string? CaptureTransactionId { get; set; }

    public string? CaptureTransactionResult { get; set; }

    public string? SubscriptionTransactionId { get; set; }

    [Precision(6)]
    public DateTime? PaidDateUtc { get; set; }

    public string? ShippingMethod { get; set; }

    public string? ShippingRateComputationMethodSystemName { get; set; }

    public string? CustomValuesXml { get; set; }

    public bool Deleted { get; set; }

    [Precision(6)]
    public DateTime CreatedOnUtc { get; set; }

    public int? RedeemedRewardPointsEntryId { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Orders")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderNote> OrderNotes { get; set; } = new List<OrderNote>();
}

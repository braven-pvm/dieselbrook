using System;
using System.Collections.Generic;
using AnqIntegrationApi.Models.Nop;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.DbContexts;

public partial class NopDbContext : DbContext
{
    public NopDbContext()
    {
    }

    public NopDbContext(DbContextOptions<NopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Address { get; set; }
    public virtual DbSet<Affiliate> Affiliate { get; set; }
    public virtual DbSet<AnqAward> AnqAward { get; set; }
    public virtual DbSet<AnqAwardShoppingCartItem> AnqAwardShoppingCartItem { get; set; }
    public virtual DbSet<AnqBooking> AnqBooking { get; set; }
    public virtual DbSet<AnqCategoryIntegration> AnqCategoryIntegration { get; set; }
    public virtual DbSet<AnqCustomShippingByWeightByTotalRecord> AnqCustomShippingByWeightByTotalRecord { get; set; }
    public virtual DbSet<AnqCustomerChange> AnqCustomerChange { get; set; }

    public virtual DbSet<AnqDiscountAppliedToCustomer> AnqDiscountAppliedToCustomer { get; set; }
    public virtual DbSet<AnqDiscountAppliedToCustomersArchive> AnqDiscountAppliedToCustomersArchive { get; set; }

    public virtual DbSet<AnqDiscountUsage> AnqDiscountUsage { get; set; }
    public virtual DbSet<AnqEvent> AnqEvent { get; set; }
    public virtual DbSet<AnqEventItem> AnqEventItem { get; set; }
    public virtual DbSet<AnqExclusiveItem> AnqExclusiveItem { get; set; }
    public virtual DbSet<AnqGift> AnqGift { get; set; }
    public virtual DbSet<AnqGiftCardAdditionalInfo> AnqGiftCardAdditionalInfo { get; set; }
    public virtual DbSet<AnqGiftsTaken> AnqGiftsTaken { get; set; }
    public virtual DbSet<AnqLookup> AnqLookup { get; set; }
    public virtual DbSet<AnqManufacturerIntegration> AnqManufacturerIntegration { get; set; }
    public virtual DbSet<AnqNewRegistration> AnqNewRegistration { get; set; }
    public virtual DbSet<AnqOffer> AnqOffer { get; set; }
    public virtual DbSet<AnqOfferList> AnqOfferList { get; set; }

    public virtual DbSet<Category> Category { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerAddress> CustomerAddressy { get; set; }
    public virtual DbSet<CustomerAttribute> CustomerAttributey { get; set; }
    public virtual DbSet<CustomerAttributeValue> CustomerAttributeValues { get; set; }
    public virtual DbSet<CustomerRole> CustomerRole { get; set; }

    public virtual DbSet<Discount> Discount { get; set; }
    public virtual DbSet<Order> Order { get; set; }
    public virtual DbSet<OrderItem> OrderItem { get; set; }
    public virtual DbSet<OrderNote> OrderNote { get; set; }
    public virtual DbSet<Product> Product { get; set; }
    public virtual DbSet<ProductCategoryMapping> ProductCategoryMapping { get; set; }

    public virtual DbSet<ProductReview> ProductReview { get; set; } = null!;
    public DbSet<AnqUserProfileAdditionalInfo> AnqUserProfileAdditionalInfos => Set<AnqUserProfileAdditionalInfo>();

    


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // OPTION 2:
        // Customer has [InverseProperty("Customer")] for these navigations,
        // but the related entity (AnqDiscountAppliedToCustomer / Archive) does not have a Customer navigation.
        // So we ignore these navigations to prevent EF from validating them.
        modelBuilder.Entity<Customer>().Ignore(c => c.AnqDiscountAppliedToCustomers);
        modelBuilder.Entity<Customer>().Ignore(c => c.AnqDiscountAppliedToCustomersArchives);
        modelBuilder.Entity<AnqUserProfileAdditionalInfo>(e =>
        {
            e.ToTable("ANQ_UserProfileAdditionalInfo", "dbo");
            e.HasKey(x => x.CustomerId);
            e.Property(x => x.WhatsappNumber).HasColumnName("WhatsappNumber");
        });
        // keep base call if you want; it does nothing harmful here
        base.OnModelCreating(modelBuilder);
    }

    public sealed class AnqUserProfileAdditionalInfo
    {
        public int CustomerId { get; set; }
        public string? WhatsappNumber { get; set; }
    }

}

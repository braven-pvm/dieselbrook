using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Annique.Plugins.Nop.Customization.Domain.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Domain.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Domain.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using FluentMigrator;
using Nop.Core;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Annique.Plugins.Nop.Customization.Data.Migration
{
    [NopMigration("2026/01/27 18:05:00:1687966", "Annique.Customization base schema")]
    public class AnniqueCustomizationSchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            //category integration
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CategoryIntegration))).Exists())
                Create.TableFor<CategoryIntegration>();

            //manufacturer integration
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ManufacturerIntegration))).Exists())
                Create.TableFor<ManufacturerIntegration>();

            //Exclusive Items
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ExclusiveItems))).Exists())
                Create.TableFor<ExclusiveItems>();

            //User profile Additional Info
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UserProfileAdditionalInfo))).Exists())
                Create.TableFor<UserProfileAdditionalInfo>();
            else
                AddTableColumn<UserProfileAdditionalInfo>();

            //Report
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Report))).Exists())
                Create.TableFor<Report>();
            else
                AddTableColumn<Report>();

            //Report Parameter
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ReportParameter))).Exists())
                Create.TableFor<ReportParameter>();

            //Report parameter Value
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ReportParameterValue))).Exists())
                Create.TableFor<ReportParameterValue>();

            //Event
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Event))).Exists())
                Create.TableFor<Event>();

            //Event Items
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(EventItems))).Exists())
                Create.TableFor<EventItems>();

            //Event Booking
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Booking))).Exists())
                Create.TableFor<Booking>();

            //Gift
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Gift))).Exists())
                Create.TableFor<Gift>();

            //Gifts Taken
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(GiftsTaken))).Exists())
                Create.TableFor<GiftsTaken>();

            //Giftcard additional info
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(GiftCardAdditionalInfo))).Exists())
                Create.TableFor<GiftCardAdditionalInfo>();

            //Customer changes
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CustomerChanges))).Exists())
                Create.TableFor<CustomerChanges>();

            //Lookups table
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Lookups))).Exists())
                Create.TableFor<Lookups>();

            //Award table
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Award))).Exists())
                Create.TableFor<Award>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(AwardShoppingCartItem))).Exists())
                Create.TableFor<AwardShoppingCartItem>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Otp))).Exists())
                Create.TableFor<Otp>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(DiscountCustomerMapping))).Exists())
                Create.TableFor<DiscountCustomerMapping>();
            else
                AddTableColumn<DiscountCustomerMapping>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(DiscountUsage))).Exists())
                Create.TableFor<DiscountUsage>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Offers))).Exists())
                Create.TableFor<Offers>();
            else
                AddTableColumn<Offers>(); 

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(OfferList))).Exists())
                Create.TableFor<OfferList>();
            else
                AddTableColumn<OfferList>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(DiscountCustomerMappingArchive))).Exists())
                Create.TableFor<DiscountCustomerMappingArchive>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CustomShippingByWeightByTotalRecord))).Exists())
                Create.TableFor<CustomShippingByWeightByTotalRecord>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ChatbotFeedback))).Exists())
                Create.TableFor<ChatbotFeedback>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(NewRegistrations))).Exists())
                Create.TableFor<NewRegistrations>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(RegistrationPageSettings))).Exists())
                Create.TableFor<RegistrationPageSettings>();
        }

        public partial class BaseNameCompatibility : INameCompatibility
        {
            public Dictionary<Type, string> TableNames => new()
            {
                {typeof(CategoryIntegration), "ANQ_CategoryIntegration" },
                {typeof(ManufacturerIntegration), "ANQ_ManufacturerIntegration" },
                {typeof(ExclusiveItems), "ANQ_ExclusiveItems" },
                {typeof(UserProfileAdditionalInfo), "ANQ_UserProfileAdditionalInfo"},
                {typeof(Report), "ANQ_Report"},
                {typeof(ReportParameter), "ANQ_ReportParameter"},
                {typeof(ReportParameterValue), "ANQ_ReportParameterValue"},
                {typeof(Event), "ANQ_Events"},
                {typeof(EventItems), "ANQ_EventItems"},
                {typeof(Booking), "ANQ_Booking"},
                {typeof(Gift), "ANQ_Gift"},
                {typeof(GiftsTaken), "ANQ_GiftsTaken"},
                {typeof(GiftCardAdditionalInfo), "ANQ_GiftCardAdditionalInfo"},
                {typeof(CustomerChanges), "ANQ_CustomerChanges"},
                {typeof(Lookups), "ANQ_Lookups"},
                {typeof(Award), "ANQ_Award"},
                {typeof(AwardShoppingCartItem), "ANQ_AwardShoppingCartItem"},
                {typeof(Otp), "ANQ_Otp"},
                {typeof(DiscountCustomerMapping), "ANQ_Discount_AppliedToCustomers"},
                {typeof(DiscountUsage), "ANQ_Discount_Usage"},
                {typeof(Offers), "ANQ_Offers"},
                {typeof(OfferList), "ANQ_OfferList"},
                {typeof(DiscountCustomerMappingArchive), "ANQ_Discount_AppliedToCustomersArchive"},
                {typeof(CustomShippingByWeightByTotalRecord), "ANQ_CustomShippingByWeightByTotalRecord"},
                {typeof(ChatbotFeedback), "ANQ_ChatbotFeedback"},
                {typeof(NewRegistrations), "ANQ_NewRegistrations"},
                {typeof(RegistrationPageSettings), "ANQ_RegistrationPageSettings"},
            };
            public Dictionary<(Type, string), string> ColumnName => new()
            {
                //do nothing
            };
        }

        public void AddTableColumn<TEntity>()
        {
            //get class proparties 
            var classPropertiesWihoutCacheKey = typeof(TEntity).GetProperties().Where(x => x.Name != nameof(BaseEntity)).ToList();

            foreach (var item in classPropertiesWihoutCacheKey)
            {
                var column = Schema.Table(NameCompatibilityManager.GetTableName(typeof(TEntity))).Column(item.Name).Exists();
                if (!column)
                {
                    if (item.PropertyType == typeof(int))
                    {
                        //add new column
                        Alter.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)))
                        .AddColumn(item.Name).AsInt32().Nullable();
                    }
                    if (item.PropertyType == typeof(string))
                    {
                        //add new column
                        Alter.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)))
                        .AddColumn(item.Name).AsString(int.MaxValue).Nullable();
                    }
                    if (item.PropertyType == typeof(bool))
                    {
                        //add new column
                        Alter.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)))
                        .AddColumn(item.Name).AsBoolean().Nullable();
                    }
                    if (item.PropertyType == typeof(decimal))
                    {
                        //add new column
                        Alter.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)))
                        .AddColumn(item.Name).AsDecimal().Nullable();
                    }
                    if (item.PropertyType == typeof(DateTime))
                    {
                        //add new column
                        Alter.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)))
                        .AddColumn(item.Name).AsDateTime().Nullable();
                    }
                }
            }
        }
    }
}

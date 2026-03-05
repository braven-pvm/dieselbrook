using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using System;
using System.Collections.Generic;
using XcellenceIT.Plugin.ProductRibbons.Domain;

namespace XcellenceIT.Plugin.ProductRibbons.Data
{
    [NopMigration("2023/04/07 10:00:17:6455422", "XcellenceIT.ProductRibbons base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : Migration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProductPictureRibbon))).Exists())
                Create.TableFor<ProductPictureRibbon>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProductRibbonRecord))).Exists())
                Create.TableFor<ProductRibbonRecord>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProductRibbonMapping))).Exists())
                Create.TableFor<ProductRibbonMapping>();
        }

        public override void Down()
        {
            //Tables names
            var productPicture = NameCompatibilityManager.GetTableName(typeof(ProductPictureRibbon));
            var productRibbonRecord = NameCompatibilityManager.GetTableName(typeof(ProductRibbonRecord));
            var productRibbonMapping = NameCompatibilityManager.GetTableName(typeof(ProductRibbonMapping));

            if (Schema.Table(productPicture).Exists())
                Delete.Table(productPicture);

            if (Schema.Table(productRibbonRecord).Exists())
                Delete.Table(productRibbonRecord);

            if (Schema.Table(productRibbonMapping).Exists())
                Delete.Table(productRibbonMapping);
        }

        #endregion

        public partial class BaseNameCompatibility : INameCompatibility
        {
            public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
            {
                { typeof(ProductPictureRibbon), "XIT_PR_ProductRibbon" },
                { typeof(ProductRibbonRecord), "XIT_PR_Ribbon" },
                { typeof(ProductRibbonMapping), "XIT_PR_RibbonProductMapping" },
            };
            public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
            {
                //do nothing
            };
        }
    }
}

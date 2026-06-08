using System;
using System.Collections.Generic;
using System.Data;
using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.DataAccess;
using Microsoft.Data.SqlClient;

namespace DBC.CareCommerce.Data.Repositories
{
    public sealed class SqlCatalogItemRepository : ICatalogItemRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlCatalogItemRepository(SqlConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _connectionFactory = connectionFactory;
        }

        public CatalogItemDto GetById(int catalogItemId)
        {
            const string sql = @"
SELECT
    CatalogItemID,
    CatalogItemGuid,
    CatalogItemType,
    ClinicalCategory,
    BillingCategory,
    FulfillmentCategory,
    DisplayName,
    ShortName,
    Description,
    SearchKeywords,
    BrandName,
    ManufacturerName,
    SKU,
    UPC,
    FeeID,
    ProductID,
    SupplementID,
    DefaultChargeAmount,
    DefaultUnits,
    Taxable,
    RevenueCategory,
    LedgerCode,
    InventoryEnabled,
    TrackQuantity,
    DefaultInventoryLocationID,
    UnitOfMeasure,
    PackageSize,
    ReorderPoint,
    ReorderQuantity,
    FullscriptEnabled,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptSKU,
    FullscriptUPC,
    FullscriptBrandID,
    FullscriptBrandName,
    FullscriptProductName,
    FullscriptVariantStatus,
    FullscriptAvailability,
    FullscriptMSRP,
    FullscriptLastSyncedDateTime,
    DefaultFulfillmentSource,
    DefaultBillingAction,
    DefaultInventoryAction,
    RequiresPatient,
    RequiresPatientCase,
    RequiresProvider,
    RequiresDosage,
    RequiresInstructions,
    DefaultDosageAmount,
    DefaultDosageFrequency,
    DefaultDosageDuration,
    DefaultDosageFormat,
    DefaultTakeWith,
    DefaultInstructions,
    CatalogStatus,
    MappingConfidence,
    NeedsReview,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.CatalogItem
WHERE CatalogItemID = @CatalogItemID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@CatalogItemID", SqlDbType.Int).Value = catalogItemId;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return MapReader(reader);
                    }
                }
            }
        }

        public CatalogItemDto GetByFeeId(int feeId)
        {
            return GetSingleByNullableInt("FeeID", feeId);
        }

        public CatalogItemDto GetByProductId(int productId)
        {
            return GetSingleByNullableInt("ProductID", productId);
        }

        public CatalogItemDto GetBySupplementId(int supplementId)
        {
            return GetSingleByNullableInt("SupplementID", supplementId);
        }

        public IList<CatalogItemDto> Search(string searchText)
        {
            List<CatalogItemDto> items = new List<CatalogItemDto>();

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                string sql;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    sql = @"
SELECT
    CatalogItemID,
    CatalogItemGuid,
    CatalogItemType,
    ClinicalCategory,
    BillingCategory,
    FulfillmentCategory,
    DisplayName,
    ShortName,
    Description,
    SearchKeywords,
    BrandName,
    ManufacturerName,
    SKU,
    UPC,
    FeeID,
    ProductID,
    SupplementID,
    DefaultChargeAmount,
    DefaultUnits,
    Taxable,
    RevenueCategory,
    LedgerCode,
    InventoryEnabled,
    TrackQuantity,
    DefaultInventoryLocationID,
    UnitOfMeasure,
    PackageSize,
    ReorderPoint,
    ReorderQuantity,
    FullscriptEnabled,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptSKU,
    FullscriptUPC,
    FullscriptBrandID,
    FullscriptBrandName,
    FullscriptProductName,
    FullscriptVariantStatus,
    FullscriptAvailability,
    FullscriptMSRP,
    FullscriptLastSyncedDateTime,
    DefaultFulfillmentSource,
    DefaultBillingAction,
    DefaultInventoryAction,
    RequiresPatient,
    RequiresPatientCase,
    RequiresProvider,
    RequiresDosage,
    RequiresInstructions,
    DefaultDosageAmount,
    DefaultDosageFrequency,
    DefaultDosageDuration,
    DefaultDosageFormat,
    DefaultTakeWith,
    DefaultInstructions,
    CatalogStatus,
    MappingConfidence,
    NeedsReview,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.CatalogItem
WHERE Active = 1
ORDER BY DisplayName;";
                }
                else
                {
                    sql = @"
SELECT
    CatalogItemID,
    CatalogItemGuid,
    CatalogItemType,
    ClinicalCategory,
    BillingCategory,
    FulfillmentCategory,
    DisplayName,
    ShortName,
    Description,
    SearchKeywords,
    BrandName,
    ManufacturerName,
    SKU,
    UPC,
    FeeID,
    ProductID,
    SupplementID,
    DefaultChargeAmount,
    DefaultUnits,
    Taxable,
    RevenueCategory,
    LedgerCode,
    InventoryEnabled,
    TrackQuantity,
    DefaultInventoryLocationID,
    UnitOfMeasure,
    PackageSize,
    ReorderPoint,
    ReorderQuantity,
    FullscriptEnabled,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptSKU,
    FullscriptUPC,
    FullscriptBrandID,
    FullscriptBrandName,
    FullscriptProductName,
    FullscriptVariantStatus,
    FullscriptAvailability,
    FullscriptMSRP,
    FullscriptLastSyncedDateTime,
    DefaultFulfillmentSource,
    DefaultBillingAction,
    DefaultInventoryAction,
    RequiresPatient,
    RequiresPatientCase,
    RequiresProvider,
    RequiresDosage,
    RequiresInstructions,
    DefaultDosageAmount,
    DefaultDosageFrequency,
    DefaultDosageDuration,
    DefaultDosageFormat,
    DefaultTakeWith,
    DefaultInstructions,
    CatalogStatus,
    MappingConfidence,
    NeedsReview,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.CatalogItem
WHERE
    Active = 1
    AND
    (
        DisplayName LIKE @SearchText
        OR ShortName LIKE @SearchText
        OR Description LIKE @SearchText
        OR SearchKeywords LIKE @SearchText
        OR BrandName LIKE @SearchText
        OR SKU LIKE @SearchText
        OR UPC LIKE @SearchText
    )
ORDER BY DisplayName;";
                }

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        AddNullableNVarChar(command, "@SearchText", "%" + searchText.Trim() + "%", 255);
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(MapReader(reader));
                        }
                    }
                }
            }

            return items;
        }

        public int Insert(CatalogItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.CatalogItemGuid == Guid.Empty)
            {
                item.CatalogItemGuid = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(item.CatalogItemType))
            {
                item.CatalogItemType = "Unknown";
            }

            if (string.IsNullOrWhiteSpace(item.DisplayName))
            {
                throw new InvalidOperationException("DisplayName is required.");
            }

            if (string.IsNullOrWhiteSpace(item.CatalogStatus))
            {
                item.CatalogStatus = "Active";
            }

            if (item.CreatedDateTime == DateTime.MinValue)
            {
                item.CreatedDateTime = DateTime.UtcNow;
            }

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
INSERT INTO dbo.CatalogItem
(
    CatalogItemGuid,
    CatalogItemType,
    ClinicalCategory,
    BillingCategory,
    FulfillmentCategory,
    DisplayName,
    ShortName,
    Description,
    SearchKeywords,
    BrandName,
    ManufacturerName,
    SKU,
    UPC,
    FeeID,
    ProductID,
    SupplementID,
    DefaultChargeAmount,
    DefaultUnits,
    Taxable,
    RevenueCategory,
    LedgerCode,
    InventoryEnabled,
    TrackQuantity,
    DefaultInventoryLocationID,
    UnitOfMeasure,
    PackageSize,
    ReorderPoint,
    ReorderQuantity,
    FullscriptEnabled,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptSKU,
    FullscriptUPC,
    FullscriptBrandID,
    FullscriptBrandName,
    FullscriptProductName,
    FullscriptVariantStatus,
    FullscriptAvailability,
    FullscriptMSRP,
    FullscriptLastSyncedDateTime,
    DefaultFulfillmentSource,
    DefaultBillingAction,
    DefaultInventoryAction,
    RequiresPatient,
    RequiresPatientCase,
    RequiresProvider,
    RequiresDosage,
    RequiresInstructions,
    DefaultDosageAmount,
    DefaultDosageFrequency,
    DefaultDosageDuration,
    DefaultDosageFormat,
    DefaultTakeWith,
    DefaultInstructions,
    CatalogStatus,
    MappingConfidence,
    NeedsReview,
    Active,
    CreatedDateTime,
    UpdatedDateTime
)
VALUES
(
    @CatalogItemGuid,
    @CatalogItemType,
    @ClinicalCategory,
    @BillingCategory,
    @FulfillmentCategory,
    @DisplayName,
    @ShortName,
    @Description,
    @SearchKeywords,
    @BrandName,
    @ManufacturerName,
    @SKU,
    @UPC,
    @FeeID,
    @ProductID,
    @SupplementID,
    @DefaultChargeAmount,
    @DefaultUnits,
    @Taxable,
    @RevenueCategory,
    @LedgerCode,
    @InventoryEnabled,
    @TrackQuantity,
    @DefaultInventoryLocationID,
    @UnitOfMeasure,
    @PackageSize,
    @ReorderPoint,
    @ReorderQuantity,
    @FullscriptEnabled,
    @FullscriptProductID,
    @FullscriptVariantID,
    @FullscriptSKU,
    @FullscriptUPC,
    @FullscriptBrandID,
    @FullscriptBrandName,
    @FullscriptProductName,
    @FullscriptVariantStatus,
    @FullscriptAvailability,
    @FullscriptMSRP,
    @FullscriptLastSyncedDateTime,
    @DefaultFulfillmentSource,
    @DefaultBillingAction,
    @DefaultInventoryAction,
    @RequiresPatient,
    @RequiresPatientCase,
    @RequiresProvider,
    @RequiresDosage,
    @RequiresInstructions,
    @DefaultDosageAmount,
    @DefaultDosageFrequency,
    @DefaultDosageDuration,
    @DefaultDosageFormat,
    @DefaultTakeWith,
    @DefaultInstructions,
    @CatalogStatus,
    @MappingConfidence,
    @NeedsReview,
    @Active,
    @CreatedDateTime,
    @UpdatedDateTime
);

SELECT CONVERT(int, SCOPE_IDENTITY());";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, item);

                    int newId = Convert.ToInt32(command.ExecuteScalar());
                    item.CatalogItemId = newId;

                    return newId;
                }
            }
        }

        public void Update(CatalogItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (!item.CatalogItemId.HasValue)
            {
                throw new InvalidOperationException("CatalogItemId is required for update.");
            }

            if (string.IsNullOrWhiteSpace(item.DisplayName))
            {
                throw new InvalidOperationException("DisplayName is required.");
            }

            item.UpdatedDateTime = DateTime.UtcNow;

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
UPDATE dbo.CatalogItem
SET
    CatalogItemGuid = @CatalogItemGuid,
    CatalogItemType = @CatalogItemType,
    ClinicalCategory = @ClinicalCategory,
    BillingCategory = @BillingCategory,
    FulfillmentCategory = @FulfillmentCategory,
    DisplayName = @DisplayName,
    ShortName = @ShortName,
    Description = @Description,
    SearchKeywords = @SearchKeywords,
    BrandName = @BrandName,
    ManufacturerName = @ManufacturerName,
    SKU = @SKU,
    UPC = @UPC,
    FeeID = @FeeID,
    ProductID = @ProductID,
    SupplementID = @SupplementID,
    DefaultChargeAmount = @DefaultChargeAmount,
    DefaultUnits = @DefaultUnits,
    Taxable = @Taxable,
    RevenueCategory = @RevenueCategory,
    LedgerCode = @LedgerCode,
    InventoryEnabled = @InventoryEnabled,
    TrackQuantity = @TrackQuantity,
    DefaultInventoryLocationID = @DefaultInventoryLocationID,
    UnitOfMeasure = @UnitOfMeasure,
    PackageSize = @PackageSize,
    ReorderPoint = @ReorderPoint,
    ReorderQuantity = @ReorderQuantity,
    FullscriptEnabled = @FullscriptEnabled,
    FullscriptProductID = @FullscriptProductID,
    FullscriptVariantID = @FullscriptVariantID,
    FullscriptSKU = @FullscriptSKU,
    FullscriptUPC = @FullscriptUPC,
    FullscriptBrandID = @FullscriptBrandID,
    FullscriptBrandName = @FullscriptBrandName,
    FullscriptProductName = @FullscriptProductName,
    FullscriptVariantStatus = @FullscriptVariantStatus,
    FullscriptAvailability = @FullscriptAvailability,
    FullscriptMSRP = @FullscriptMSRP,
    FullscriptLastSyncedDateTime = @FullscriptLastSyncedDateTime,
    DefaultFulfillmentSource = @DefaultFulfillmentSource,
    DefaultBillingAction = @DefaultBillingAction,
    DefaultInventoryAction = @DefaultInventoryAction,
    RequiresPatient = @RequiresPatient,
    RequiresPatientCase = @RequiresPatientCase,
    RequiresProvider = @RequiresProvider,
    RequiresDosage = @RequiresDosage,
    RequiresInstructions = @RequiresInstructions,
    DefaultDosageAmount = @DefaultDosageAmount,
    DefaultDosageFrequency = @DefaultDosageFrequency,
    DefaultDosageDuration = @DefaultDosageDuration,
    DefaultDosageFormat = @DefaultDosageFormat,
    DefaultTakeWith = @DefaultTakeWith,
    DefaultInstructions = @DefaultInstructions,
    CatalogStatus = @CatalogStatus,
    MappingConfidence = @MappingConfidence,
    NeedsReview = @NeedsReview,
    Active = @Active,
    CreatedDateTime = @CreatedDateTime,
    UpdatedDateTime = @UpdatedDateTime
WHERE CatalogItemID = @CatalogItemID;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, item);
                    command.Parameters.Add("@CatalogItemID", SqlDbType.Int).Value = item.CatalogItemId.Value;

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Catalog item was not found.");
                    }
                }
            }
        }

        private CatalogItemDto GetSingleByNullableInt(string columnName, int value)
        {
            string sql = @"
SELECT TOP 1
    CatalogItemID,
    CatalogItemGuid,
    CatalogItemType,
    ClinicalCategory,
    BillingCategory,
    FulfillmentCategory,
    DisplayName,
    ShortName,
    Description,
    SearchKeywords,
    BrandName,
    ManufacturerName,
    SKU,
    UPC,
    FeeID,
    ProductID,
    SupplementID,
    DefaultChargeAmount,
    DefaultUnits,
    Taxable,
    RevenueCategory,
    LedgerCode,
    InventoryEnabled,
    TrackQuantity,
    DefaultInventoryLocationID,
    UnitOfMeasure,
    PackageSize,
    ReorderPoint,
    ReorderQuantity,
    FullscriptEnabled,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptSKU,
    FullscriptUPC,
    FullscriptBrandID,
    FullscriptBrandName,
    FullscriptProductName,
    FullscriptVariantStatus,
    FullscriptAvailability,
    FullscriptMSRP,
    FullscriptLastSyncedDateTime,
    DefaultFulfillmentSource,
    DefaultBillingAction,
    DefaultInventoryAction,
    RequiresPatient,
    RequiresPatientCase,
    RequiresProvider,
    RequiresDosage,
    RequiresInstructions,
    DefaultDosageAmount,
    DefaultDosageFrequency,
    DefaultDosageDuration,
    DefaultDosageFormat,
    DefaultTakeWith,
    DefaultInstructions,
    CatalogStatus,
    MappingConfidence,
    NeedsReview,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.CatalogItem
WHERE Active = 1
    AND " + columnName + @" = @Value
ORDER BY CatalogItemID DESC;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Value", SqlDbType.Int).Value = value;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return MapReader(reader);
                    }
                }
            }
        }

        private static CatalogItemDto MapReader(SqlDataReader reader)
        {
            return new CatalogItemDto
            {
                CatalogItemId = GetNullableInt(reader, "CatalogItemID"),
                CatalogItemGuid = GetGuid(reader, "CatalogItemGuid"),
                CatalogItemType = GetString(reader, "CatalogItemType"),
                ClinicalCategory = GetNullableString(reader, "ClinicalCategory"),
                BillingCategory = GetNullableString(reader, "BillingCategory"),
                FulfillmentCategory = GetNullableString(reader, "FulfillmentCategory"),
                DisplayName = GetString(reader, "DisplayName"),
                ShortName = GetNullableString(reader, "ShortName"),
                Description = GetNullableString(reader, "Description"),
                SearchKeywords = GetNullableString(reader, "SearchKeywords"),
                BrandName = GetNullableString(reader, "BrandName"),
                ManufacturerName = GetNullableString(reader, "ManufacturerName"),
                Sku = GetNullableString(reader, "SKU"),
                Upc = GetNullableString(reader, "UPC"),
                FeeId = GetNullableInt(reader, "FeeID"),
                ProductId = GetNullableInt(reader, "ProductID"),
                SupplementId = GetNullableInt(reader, "SupplementID"),
                DefaultChargeAmount = GetNullableDecimal(reader, "DefaultChargeAmount"),
                DefaultUnits = GetNullableDecimal(reader, "DefaultUnits"),
                Taxable = GetNullableBool(reader, "Taxable"),
                RevenueCategory = GetNullableString(reader, "RevenueCategory"),
                LedgerCode = GetNullableString(reader, "LedgerCode"),
                InventoryEnabled = GetBool(reader, "InventoryEnabled"),
                TrackQuantity = GetBool(reader, "TrackQuantity"),
                DefaultInventoryLocationId = GetNullableInt(reader, "DefaultInventoryLocationID"),
                UnitOfMeasure = GetNullableString(reader, "UnitOfMeasure"),
                PackageSize = GetNullableString(reader, "PackageSize"),
                ReorderPoint = GetNullableDecimal(reader, "ReorderPoint"),
                ReorderQuantity = GetNullableDecimal(reader, "ReorderQuantity"),
                FullscriptEnabled = GetBool(reader, "FullscriptEnabled"),
                FullscriptProductId = GetNullableString(reader, "FullscriptProductID"),
                FullscriptVariantId = GetNullableString(reader, "FullscriptVariantID"),
                FullscriptSku = GetNullableString(reader, "FullscriptSKU"),
                FullscriptUpc = GetNullableString(reader, "FullscriptUPC"),
                FullscriptBrandId = GetNullableString(reader, "FullscriptBrandID"),
                FullscriptBrandName = GetNullableString(reader, "FullscriptBrandName"),
                FullscriptProductName = GetNullableString(reader, "FullscriptProductName"),
                FullscriptVariantStatus = GetNullableString(reader, "FullscriptVariantStatus"),
                FullscriptAvailability = GetNullableString(reader, "FullscriptAvailability"),
                FullscriptMsrp = GetNullableDecimal(reader, "FullscriptMSRP"),
                FullscriptLastSyncedDateTime = GetNullableDateTime(reader, "FullscriptLastSyncedDateTime"),
                DefaultFulfillmentSource = ParseFulfillmentSource(GetString(reader, "DefaultFulfillmentSource")),
                DefaultBillingAction = ParseBillingAction(GetString(reader, "DefaultBillingAction")),
                DefaultInventoryAction = ParseInventoryAction(GetString(reader, "DefaultInventoryAction")),
                RequiresPatient = GetBool(reader, "RequiresPatient"),
                RequiresPatientCase = GetBool(reader, "RequiresPatientCase"),
                RequiresProvider = GetBool(reader, "RequiresProvider"),
                RequiresDosage = GetBool(reader, "RequiresDosage"),
                RequiresInstructions = GetBool(reader, "RequiresInstructions"),
                DefaultDosageAmount = GetNullableString(reader, "DefaultDosageAmount"),
                DefaultDosageFrequency = GetNullableString(reader, "DefaultDosageFrequency"),
                DefaultDosageDuration = GetNullableString(reader, "DefaultDosageDuration"),
                DefaultDosageFormat = GetNullableString(reader, "DefaultDosageFormat"),
                DefaultTakeWith = GetNullableString(reader, "DefaultTakeWith"),
                DefaultInstructions = GetNullableString(reader, "DefaultInstructions"),
                CatalogStatus = GetString(reader, "CatalogStatus"),
                MappingConfidence = GetNullableString(reader, "MappingConfidence"),
                NeedsReview = GetBool(reader, "NeedsReview"),
                Active = GetBool(reader, "Active"),
                CreatedDateTime = GetDateTime(reader, "CreatedDateTime"),
                UpdatedDateTime = GetNullableDateTime(reader, "UpdatedDateTime")
            };
        }

        private static void AddParameters(SqlCommand command, CatalogItemDto item)
        {
            command.Parameters.Add("@CatalogItemGuid", SqlDbType.UniqueIdentifier).Value = item.CatalogItemGuid;
            AddNVarChar(command, "@CatalogItemType", item.CatalogItemType, 50);
            AddNullableNVarChar(command, "@ClinicalCategory", item.ClinicalCategory, 50);
            AddNullableNVarChar(command, "@BillingCategory", item.BillingCategory, 50);
            AddNullableNVarChar(command, "@FulfillmentCategory", item.FulfillmentCategory, 50);
            AddNVarChar(command, "@DisplayName", item.DisplayName, 255);
            AddNullableNVarChar(command, "@ShortName", item.ShortName, 100);
            AddNullableNVarChar(command, "@Description", item.Description, -1);
            AddNullableNVarChar(command, "@SearchKeywords", item.SearchKeywords, -1);
            AddNullableNVarChar(command, "@BrandName", item.BrandName, 255);
            AddNullableNVarChar(command, "@ManufacturerName", item.ManufacturerName, 255);
            AddNullableNVarChar(command, "@SKU", item.Sku, 100);
            AddNullableNVarChar(command, "@UPC", item.Upc, 100);
            AddNullableInt(command, "@FeeID", item.FeeId);
            AddNullableInt(command, "@ProductID", item.ProductId);
            AddNullableInt(command, "@SupplementID", item.SupplementId);
            AddNullableDecimal(command, "@DefaultChargeAmount", item.DefaultChargeAmount, 18, 2);
            AddNullableDecimal(command, "@DefaultUnits", item.DefaultUnits, 18, 4);
            AddNullableBool(command, "@Taxable", item.Taxable);
            AddNullableNVarChar(command, "@RevenueCategory", item.RevenueCategory, 100);
            AddNullableNVarChar(command, "@LedgerCode", item.LedgerCode, 100);
            AddBool(command, "@InventoryEnabled", item.InventoryEnabled);
            AddBool(command, "@TrackQuantity", item.TrackQuantity);
            AddNullableInt(command, "@DefaultInventoryLocationID", item.DefaultInventoryLocationId);
            AddNullableNVarChar(command, "@UnitOfMeasure", item.UnitOfMeasure, 50);
            AddNullableNVarChar(command, "@PackageSize", item.PackageSize, 100);
            AddNullableDecimal(command, "@ReorderPoint", item.ReorderPoint, 18, 4);
            AddNullableDecimal(command, "@ReorderQuantity", item.ReorderQuantity, 18, 4);
            AddBool(command, "@FullscriptEnabled", item.FullscriptEnabled);
            AddNullableNVarChar(command, "@FullscriptProductID", item.FullscriptProductId, 100);
            AddNullableNVarChar(command, "@FullscriptVariantID", item.FullscriptVariantId, 100);
            AddNullableNVarChar(command, "@FullscriptSKU", item.FullscriptSku, 100);
            AddNullableNVarChar(command, "@FullscriptUPC", item.FullscriptUpc, 100);
            AddNullableNVarChar(command, "@FullscriptBrandID", item.FullscriptBrandId, 100);
            AddNullableNVarChar(command, "@FullscriptBrandName", item.FullscriptBrandName, 255);
            AddNullableNVarChar(command, "@FullscriptProductName", item.FullscriptProductName, 255);
            AddNullableNVarChar(command, "@FullscriptVariantStatus", item.FullscriptVariantStatus, 100);
            AddNullableNVarChar(command, "@FullscriptAvailability", item.FullscriptAvailability, 100);
            AddNullableDecimal(command, "@FullscriptMSRP", item.FullscriptMsrp, 18, 2);
            AddNullableDateTime(command, "@FullscriptLastSyncedDateTime", item.FullscriptLastSyncedDateTime);
            AddNVarChar(command, "@DefaultFulfillmentSource", item.DefaultFulfillmentSource.ToString(), 50);
            AddNVarChar(command, "@DefaultBillingAction", item.DefaultBillingAction.ToString(), 50);
            AddNVarChar(command, "@DefaultInventoryAction", item.DefaultInventoryAction.ToString(), 50);
            AddBool(command, "@RequiresPatient", item.RequiresPatient);
            AddBool(command, "@RequiresPatientCase", item.RequiresPatientCase);
            AddBool(command, "@RequiresProvider", item.RequiresProvider);
            AddBool(command, "@RequiresDosage", item.RequiresDosage);
            AddBool(command, "@RequiresInstructions", item.RequiresInstructions);
            AddNullableNVarChar(command, "@DefaultDosageAmount", item.DefaultDosageAmount, 100);
            AddNullableNVarChar(command, "@DefaultDosageFrequency", item.DefaultDosageFrequency, 100);
            AddNullableNVarChar(command, "@DefaultDosageDuration", item.DefaultDosageDuration, 100);
            AddNullableNVarChar(command, "@DefaultDosageFormat", item.DefaultDosageFormat, 100);
            AddNullableNVarChar(command, "@DefaultTakeWith", item.DefaultTakeWith, 100);
            AddNullableNVarChar(command, "@DefaultInstructions", item.DefaultInstructions, -1);
            AddNVarChar(command, "@CatalogStatus", item.CatalogStatus, 50);
            AddNullableNVarChar(command, "@MappingConfidence", item.MappingConfidence, 50);
            AddBool(command, "@NeedsReview", item.NeedsReview);
            AddBool(command, "@Active", item.Active);
            AddDateTime(command, "@CreatedDateTime", item.CreatedDateTime);
            AddNullableDateTime(command, "@UpdatedDateTime", item.UpdatedDateTime);
        }

        private static FulfillmentSource ParseFulfillmentSource(string value)
        {
            FulfillmentSource parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : FulfillmentSource.None;
        }

        private static BillingAction ParseBillingAction(string value)
        {
            BillingAction parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : BillingAction.NoBilling;
        }

        private static InventoryAction ParseInventoryAction(string value)
        {
            InventoryAction parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : InventoryAction.None;
        }

        public CatalogItemDto GetByFullscriptVariantId(string fullscriptVariantId)
        {
            if (string.IsNullOrWhiteSpace(fullscriptVariantId))
            {
                return null;
            }

            const string sql = @"
SELECT TOP 1
    CatalogItemID
FROM dbo.CatalogItem
WHERE
    Active = 1
    AND FullscriptVariantID = @FullscriptVariantID
ORDER BY CatalogItemID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@FullscriptVariantID", SqlDbType.NVarChar, 200).Value = fullscriptVariantId;

                    object result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        return null;
                    }

                    int catalogItemId = Convert.ToInt32(result);
                    return GetById(catalogItemId);
                }
            }
        }

        private static void AddNVarChar(SqlCommand command, string parameterName, string value, int size)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.NVarChar, size);
            parameter.Value = value;
        }

        private static void AddNullableNVarChar(SqlCommand command, string parameterName, string value, int size)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.NVarChar, size);
            parameter.Value = string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
        }

        private static void AddNullableInt(SqlCommand command, string parameterName, int? value)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Int);
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static void AddNullableDecimal(SqlCommand command, string parameterName, decimal? value, byte precision, byte scale)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static void AddNullableBool(SqlCommand command, string parameterName, bool? value)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Bit);
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static void AddBool(SqlCommand command, string parameterName, bool value)
        {
            command.Parameters.Add(parameterName, SqlDbType.Bit).Value = value;
        }

        private static void AddDateTime(SqlCommand command, string parameterName, DateTime value)
        {
            command.Parameters.Add(parameterName, SqlDbType.DateTime2).Value = value;
        }

        private static void AddNullableDateTime(SqlCommand command, string parameterName, DateTime? value)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.DateTime2);
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static string GetString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.GetString(ordinal);
        }

        private static string GetNullableString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (int?)null : reader.GetInt32(ordinal);
        }

        private static Guid GetGuid(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? Guid.Empty : reader.GetGuid(ordinal);
        }

        private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (decimal?)null : reader.GetDecimal(ordinal);
        }

        private static bool GetBool(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }

        private static bool? GetNullableBool(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (bool?)null : reader.GetBoolean(ordinal);
        }

        private static DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.GetDateTime(ordinal);
        }

        private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (DateTime?)null : reader.GetDateTime(ordinal);
        }
    }
}
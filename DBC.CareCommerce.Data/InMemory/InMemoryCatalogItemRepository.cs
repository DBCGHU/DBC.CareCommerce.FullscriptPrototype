using System;
using System.Collections.Generic;
using System.Linq;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.Data.InMemory
{
    public class InMemoryCatalogItemRepository : ICatalogItemRepository
    {
        private readonly List<CatalogItemDto> _items;
        private int _nextId;

        public InMemoryCatalogItemRepository()
        {
            _items = new List<CatalogItemDto>();
            _nextId = 1;
        }

        public CatalogItemDto GetById(int catalogItemId)
        {
            return _items.FirstOrDefault(x => x.CatalogItemId == catalogItemId);
        }

        public CatalogItemDto GetByFeeId(int feeId)
        {
            return _items.FirstOrDefault(x => x.FeeId == feeId);
        }

        public CatalogItemDto GetByProductId(int productId)
        {
            return _items.FirstOrDefault(x => x.ProductId == productId);
        }

        public CatalogItemDto GetBySupplementId(int supplementId)
        {
            return _items.FirstOrDefault(x => x.SupplementId == supplementId);
        }

        public IList<CatalogItemDto> Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _items.Where(x => x.Active).ToList();
            }

            var value = searchText.Trim();

            return _items
                .Where(x =>
                    x.Active &&
                    (
                        Contains(x.DisplayName, value) ||
                        Contains(x.ShortName, value) ||
                        Contains(x.Description, value) ||
                        Contains(x.SearchKeywords, value) ||
                        Contains(x.BrandName, value) ||
                        Contains(x.Sku, value) ||
                        Contains(x.Upc, value)
                    ))
                .ToList();
        }

        public int Insert(CatalogItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.CatalogItemId.HasValue && GetById(item.CatalogItemId.Value) != null)
            {
                throw new InvalidOperationException("Catalog item already exists.");
            }

            if (!item.CatalogItemId.HasValue || item.CatalogItemId.Value <= 0)
            {
                item.CatalogItemId = _nextId;
                _nextId += 1;
            }
            else if (item.CatalogItemId.Value >= _nextId)
            {
                _nextId = item.CatalogItemId.Value + 1;
            }

            if (item.CatalogItemGuid == Guid.Empty)
            {
                item.CatalogItemGuid = Guid.NewGuid();
            }

            if (item.CreatedDateTime == DateTime.MinValue)
            {
                item.CreatedDateTime = DateTime.UtcNow;
            }

            _items.Add(item);

            return item.CatalogItemId.Value;
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

            var existing = GetById(item.CatalogItemId.Value);

            if (existing == null)
            {
                throw new InvalidOperationException("Catalog item was not found.");
            }

            var index = _items.IndexOf(existing);
            item.UpdatedDateTime = DateTime.UtcNow;
            _items[index] = item;
        }

        public void Seed(CatalogItemDto item)
        {
            Insert(item);
        }

        public IList<CatalogItemDto> GetAll()
        {
            return _items.ToList();
        }

        private static bool Contains(string source, string value)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

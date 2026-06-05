using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Data.Repositories
{
    public interface ICatalogItemRepository
    {
        CatalogItemDto GetById(int catalogItemId);

        CatalogItemDto GetByFeeId(int feeId);

        CatalogItemDto GetByProductId(int productId);

        CatalogItemDto GetBySupplementId(int supplementId);

        IList<CatalogItemDto> Search(string searchText);

        int Insert(CatalogItemDto item);

        void Update(CatalogItemDto item);
    }
}
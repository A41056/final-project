using Catalog.API.Products.SearchProduct;

namespace Catalog.API.Services
{
    public interface IElasticSearchService
    {
        Task IndexProductAsync(ProductElasticModel product, CancellationToken cancellationToken = default);
        Task UpdateProductAsync(ProductElasticModel product, CancellationToken cancellationToken = default);
        Task<SearchProductResponse> SearchProductsAsync(SearchProductRequest request);
    }
}

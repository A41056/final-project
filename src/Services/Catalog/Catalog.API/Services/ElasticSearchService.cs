using Catalog.API.Products.SearchProduct;
using Nest;

namespace Catalog.API.Services
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly IElasticClient _client;
        private const string IndexName = "products";

        public ElasticSearchService(IElasticClient client)
        {
            _client = client;
        }

        public async Task IndexProductAsync(ProductElasticModel product, CancellationToken cancellationToken = default)
        {
            await _client.IndexAsync(product, i => i.Index(IndexName), cancellationToken);
        }

        public async Task UpdateProductAsync(ProductElasticModel product, CancellationToken cancellationToken = default)
        {
            var response = await _client.IndexAsync(product, i => i.Index(IndexName), cancellationToken);
            if (!response.IsValid)
            {
                Console.WriteLine($"ElasticSearch index failed: {response.DebugInformation}");
                throw new Exception($"ElasticSearch index failed: {response.ServerError?.Error?.Reason ?? response.DebugInformation}");
            }
        }

        public async Task<SearchProductResponse> SearchProductsAsync(SearchProductRequest request)
        {
            var mustQueries = new List<QueryContainer>();

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                mustQueries.Add(new QueryStringQuery { Fields = "name,description", Query = request.Query });
            }
            if (request.Tags != null && request.Tags.Any())
            {
                mustQueries.Add(new TermsQuery { Field = "tags.keyword", Terms = request.Tags });
            }
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                mustQueries.Add(new TermsQuery { Field = "categoryIds", Terms = request.CategoryIds.Select(id => (object)Guid.Parse(id)) });
            }
            if (request.IsHot.HasValue)
            {
                mustQueries.Add(new TermQuery { Field = "isHot", Value = request.IsHot.Value });
            }
            if (request.IsActive.HasValue)
            {
                mustQueries.Add(new TermQuery { Field = "isActive", Value = request.IsActive.Value });
            }

            var searchResponse = await _client.SearchAsync<ProductElasticModel>(s => s
                .Index(IndexName)
                .Query(q => q.Bool(b => b.Must(mustQueries.ToArray())))
                .From(((request.PageNumber ?? 1) - 1) * (request.PageSize ?? 10))
                .Size(request.PageSize ?? 10)
            );

            return new SearchProductResponse(searchResponse.Documents, (int)searchResponse.Total);
        }
    }
}

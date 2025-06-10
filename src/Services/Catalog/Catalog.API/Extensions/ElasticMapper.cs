namespace Catalog.API.Extensions
{
    public static class ElasticMapper
    {
        public static ProductElasticVariant ToElastic(this ProductVariant variant)
        {
            return new ProductElasticVariant
            {
                Price = variant.Price,
                DiscountPrice = variant.DiscountPrice,
                StockCount = variant.StockCount,
                Properties = variant.Properties?.Select(p => new VariantElasticProperty
                {
                    Type = p.Type,
                    Value = p.Value,
                    Image = p.Image
                }).ToList() ?? new List<VariantElasticProperty>()
            };
        }
    }
}

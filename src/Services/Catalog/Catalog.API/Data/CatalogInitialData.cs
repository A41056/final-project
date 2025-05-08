using Marten.Schema;

namespace Catalog.API.Data;

public class CatalogInitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();

        // Kiểm tra xem đã có dữ liệu chưa
        if (await session.Query<Product>().AnyAsync(cancellation))
            return;

        // Thêm các Category trước
        var categories = GetPreconfiguredCategories();
        session.Store(categories.ToArray());
        await session.SaveChangesAsync(cancellation);

        // Thêm các Product với CategoryIds
        var products = GetPreconfiguredProducts(categories);
        session.Store(products.ToArray());
        await session.SaveChangesAsync(cancellation);
    }

    private static IEnumerable<Category> GetPreconfiguredCategories() => new List<Category>
    {
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Smart Phone",
            Slug = "smart-phone",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        },
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "White Appliances",
            Slug = "white-appliances",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        },
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Home Kitchen",
            Slug = "home-kitchen",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        },
        new Category
        {
            Id = Guid.NewGuid(),
            Name = "Camera",
            Slug = "camera",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        }
    };

    private static IEnumerable<Product> GetPreconfiguredProducts(IEnumerable<Category> categories)
    {
        var categoryDict = categories.ToDictionary(c => c.Name, c => c.Id);

        return new List<Product>
    {
        new Product
        {
            Id = new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61"),
            Name = "IPhone X",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-1.png" },
            CategoryIds = new List<Guid> { categoryDict["Smart Phone"] },
            IsHot = false,
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "64GB" } },
                    Price = 950.00M,
                    StockCount = 100
                },
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "256GB" } },
                    Price = 1150.00M,
                    StockCount = 100
                }
            },
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        },
        new Product
        {
            Id = new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914"),
            Name = "Samsung 10",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-2.png" },
            CategoryIds = new List<Guid> { categoryDict["Smart Phone"] },
            IsHot = false,
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "64GB" } },
                    Price = 850.00M,
                    StockCount = 100
                },
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "256GB" } },
                    Price = 1050.00M,
                    StockCount = 100
                }
            },
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        },
        new Product
        {
            Id = new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8"),
            Name = "Huawei Plus",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-3.png" },
            CategoryIds = new List<Guid> { categoryDict["White Appliances"] },
            IsHot = false,
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "64GB" } },
                    Price = 750.00M,
                    StockCount = 100
                },
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "256GB" } },
                    Price = 950.00M,
                    StockCount = 100
                }
            },
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        },
        new Product
        {
            Id = new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27"),
            Name = "Xiaomi Mi 9",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-4.png" },
            CategoryIds = new List<Guid> { categoryDict["White Appliances"] },
            IsHot = false,
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "64GB" } },
                    Price = 550.00M,
                    StockCount = 100
                },
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "256GB" } },
                    Price = 750.00M,
                    StockCount = 100
                }
            },
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        },
        new Product
        {
            Id = new Guid("b786103d-c621-4f5a-b498-23452610f88c"),
            Name = "HTC U11+ Plus",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-5.png" },
            CategoryIds = new List<Guid> { categoryDict["Smart Phone"] },
            IsHot = false,
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "64GB" } },
                    Price = 450.00M,
                    StockCount = 100
                },
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "256GB" } },
                    Price = 650.00M,
                    StockCount = 100
                }
            },
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        },
        new Product
        {
            Id = new Guid("c4bbc4a2-4555-45d8-97cc-2a99b2167bff"),
            Name = "LG G7 ThinQ",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-6.png" },
            CategoryIds = new List<Guid> { categoryDict["Home Kitchen"] },
            IsHot = false,
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "64GB" } },
                    Price = 350.00M,
                    StockCount = 100
                },
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "256GB" } },
                    Price = 550.00M,
                    StockCount = 100
                }
            },
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        },
        new Product
        {
            Id = new Guid("93170c85-7795-489c-8e8f-7dcf3b4f4188"),
            Name = "Panasonic Lumix",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-6.png" },
            CategoryIds = new List<Guid> { categoryDict["Camera"] },
            IsHot = false,
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "64GB" } },
                    Price = 250.00M,
                    StockCount = 100
                },
                new ProductVariant
                {
                    Properties = new List<VariantProperty> { new VariantProperty { Type = "Storage", Value = "256GB" } },
                    Price = 450.00M,
                    StockCount = 100
                }
            },
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        }
    };
    }
}
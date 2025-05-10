using Marten.Schema;

namespace Catalog.API.Data;

public class CatalogInitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();

        if (await session.Query<Product>().AnyAsync(cancellation))
            return;

        var categories = GetPreconfiguredCategories();
        session.Store(categories.ToArray());
        await session.SaveChangesAsync(cancellation);

        var products = GetPreconfiguredProducts(categories);
        session.Store(products.ToArray());
        await session.SaveChangesAsync(cancellation);
    }

    private static IEnumerable<Category> GetPreconfiguredCategories() => new List<Category>
    {
        new Category
        {
            Id = Guid.Parse("5c8634c2-452c-4b38-8f13-b9d268f2f72f"),
            Name = "Shop",
            Slug = "shop",
            ParentId = null
        },
        new Category
        {
            Id = Guid.Parse("1e1bfc43-cd29-4e42-9f39-415f3e6b714c"),
            Name = "Đồ Điện Tử",
            Slug = "do-dien-tu",
            ParentId = Guid.Parse("5c8634c2-452c-4b38-8f13-b9d268f2f72f")
        },
        new Category
        {
            Id = Guid.Parse("1a839a3a-f960-4a41-b0a5-0f8013dd2b60"),
            Name = "Điện thoại",
            Slug = "dien-thoai",
            ParentId = Guid.Parse("1e1bfc43-cd29-4e42-9f39-415f3e6b714c")
        },
        new Category
        {
            Id = Guid.Parse("cecc68db-bd29-495e-a951-58c172137f2e"),
            Name = "Laptop",
            Slug = "laptop",
            ParentId = Guid.Parse("1e1bfc43-cd29-4e42-9f39-415f3e6b714c")
        },
        new Category
        {
            Id = Guid.Parse("b2c09a25-d61a-4909-9a91-b7f2e7f0ef9f"),
            Name = "Máy ảnh",
            Slug = "may-anh",
            ParentId = Guid.Parse("1e1bfc43-cd29-4e42-9f39-415f3e6b714c")
        },
        new Category
        {
            Id = Guid.Parse("23d32d58-339c-4a3b-9b0e-9d24183ac5e7"),
            Name = "Gia Dụng",
            Slug = "gia-dung",
            ParentId = Guid.Parse("5c8634c2-452c-4b38-8f13-b9d268f2f72f")
        },
        new Category
        {
            Id = Guid.Parse("bd1543d5-cd3d-46e7-8fae-e63de9422d53"),
            Name = "Nhà bếp",
            Slug = "nha-bep",
            ParentId = Guid.Parse("23d32d58-339c-4a3b-9b0e-9d24183ac5e7")
        },
        new Category
        {
            Id = Guid.Parse("2f6b9bfa-59ae-41d4-93a0-d5d0a71d1ad4"),
            Name = "Thiết bị vệ sinh",
            Slug = "thiet-bi-ve-sinh",
            ParentId = Guid.Parse("23d32d58-339c-4a3b-9b0e-9d24183ac5e7")
        },
        new Category
        {
            Id = Guid.Parse("e8714c44-679c-4387-9bc8-2efcc6ffce94"),
            Name = "Thời Trang",
            Slug = "thoi-trang",
            ParentId = Guid.Parse("5c8634c2-452c-4b38-8f13-b9d268f2f72f")
        },
        new Category
        {
            Id = Guid.Parse("ff227ab7-bf56-4dd3-b6b2-9a0f1d2f33e5"),
            Name = "Nam",
            Slug = "nam",
            ParentId = Guid.Parse("e8714c44-679c-4387-9bc8-2efcc6ffce94")
        },
        new Category
        {
            Id = Guid.Parse("ddc52cd2-98d7-43dc-9936-3a32621605e8"),
            Name = "Nữ",
            Slug = "nu",
            ParentId = Guid.Parse("e8714c44-679c-4387-9bc8-2efcc6ffce94")
        }
    };

    private static IEnumerable<Product> GetPreconfiguredProducts(IEnumerable<Category> categories)
    {
        var categoryDict = categories.ToDictionary(c => c.Name, c => c.Id);

        return new List<Product>
    {
        new Product
        {
            Id = new Guid("1a839a3a-f960-4a41-b0a5-0f8013dd2b60"),
            Name = "IPhone X",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-1.png" },
            CategoryIds = new List<Guid> { categoryDict["Điện thoại"] },
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
            Id = new Guid("1a839a3a-f960-4a41-b0a5-0f8013dd2b60"),
            Name = "Samsung 10",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-2.png" },
            CategoryIds = new List<Guid> { categoryDict["Điện thoại"] },
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
            Id = new Guid("1a839a3a-f960-4a41-b0a5-0f8013dd2b60"),
            Name = "Huawei Plus",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-3.png" },
            CategoryIds = new List<Guid> { categoryDict["Gia Dụng"] },
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
            Id = new Guid("1a839a3a-f960-4a41-b0a5-0f8013dd2b60"),
            Name = "Xiaomi Mi 9",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-4.png" },
            CategoryIds = new List<Guid> { categoryDict["Gia Dụng"] },
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
            Id = new Guid("1a839a3a-f960-4a41-b0a5-0f8013dd2b60"),
            Name = "HTC U11+ Plus",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-5.png" },
            CategoryIds = new List<Guid> { categoryDict["Điện thoại"] },
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
            Id = new Guid("1a839a3a-f960-4a41-b0a5-0f8013dd2b60"),
            Name = "LG G7 ThinQ",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFiles = new List<string> { "product-6.png" },
            CategoryIds = new List<Guid> { categoryDict["Nhà bếp"] },
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
    };
    }
}
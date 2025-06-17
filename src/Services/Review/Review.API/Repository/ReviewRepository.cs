using JasperFx.Core;
namespace Review.API.Repository;

public class ReviewRepository: IReviewRepository
{
    private readonly IDocumentSession _session;
    
    public ReviewRepository(IDocumentSession session)
    {
        _session = session;
    }

    public Task<Models.Review> GetByIdAsync(Guid id)
    {
        return _session.LoadAsync<Models.Review>(id);
    }

    public async Task<List<Models.Review>> GetByProductIdAsync(Guid productId)
    {
        return (await _session.Query<Models.Review>()
            .Where(r => r.ProductId == productId && r.IsActive)
            .ToListAsync()).ToList();
    }

    public Task InsertAsync(Models.Review review)
    {
        _session.Store(review);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Models.Review review)
    {
        _session.Store(review);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var review = await _session.LoadAsync<Models.Review>(id);
        if (review != null)
        {
            review.IsActive = false;
            review.Modified = DateTime.UtcNow;
            _session.Store(review);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _session.SaveChangesAsync(cancellationToken);
    }
}
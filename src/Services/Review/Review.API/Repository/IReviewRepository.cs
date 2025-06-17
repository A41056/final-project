namespace Review.API.Repository;

public interface IReviewRepository
{
    Task<Models.Review> GetByIdAsync(Guid id);
    Task<List<Models.Review>> GetByProductIdAsync(Guid productId);
    Task InsertAsync(Models.Review review);
    Task UpdateAsync(Models.Review review);
    Task DeleteAsync(Guid id);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
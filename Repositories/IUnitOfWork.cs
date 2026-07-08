namespace Repositories
{
  // to avoid exposing DbContext in services
  public interface IUnitOfWork
  {
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
  }
}

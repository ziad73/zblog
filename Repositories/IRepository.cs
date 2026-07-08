namespace Repositories
{
  // where to use Savechanges() inside service or repository?
  // In real-world apps, the most common best practice is:
    // Repository methods change tracked entities
    // Service/application layer owns SaveChangesAsync(), if each service do just one CRUD operation, then it's okay to use it anywhere!
    // One business use case = one commit/SaveChangesAsync()





  // IRepository<T> is interface is the generic implementation of the Repository Pattern
  // less duplicated code, consistent API across repositories, easy to add shared CRUD methods once
  public interface IRepository<T> where T : class
  {
    // ignored for now, search about it in future
    // IQueryable<T> Query();

    // CancellationToken is used to cancel the async operation that may take time (e.g. database queries, HTTP requests, file operations, background jobs), it's commonly passed into async database calls so the request can be canceled if: 
      // - the client disconnects/cancels the request
      // - the request times out 
      // - the server wants to abort the operation
    // with default makes the parameter optional
    // If it’s just quick local logic, you can skip it.
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    // For Update and Delete: they just mark state, no I/O happens yet
      // - repository marks the entity as updated or deleted
      // - service calls SaveChangesAsync() once at the end
    void Update(T entity);
    void Delete(T entity);
  }
}

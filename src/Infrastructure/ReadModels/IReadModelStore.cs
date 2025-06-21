using Domain.ValueObjects;

namespace Infrastructure.ReadModels;

public interface IReadModelStore
{
    Task<T?> GetByIdAsync<T>(string id)
        where T : class;
    Task<IEnumerable<T>> GetAllAsync<T>()
        where T : class;
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        where T : class;
    Task<int> ExecuteAsync(string sql, object? parameters = null);
    Task<T> InsertAsync<T>(T entity)
        where T : class;
    Task<T> UpdateAsync<T>(T entity)
        where T : class;
    Task<bool> DeleteAsync<T>(string id)
        where T : class;
    Task<bool> ExistsAsync<T>(string id)
        where T : class;
    Task<int> CountAsync<T>()
        where T : class;
    Task<IEnumerable<T>> GetByAccountIdAsync<T>(AccountId accountId)
        where T : class;
    Task<IEnumerable<T>> GetByCustomerIdAsync<T>(CustomerId customerId)
        where T : class;
}

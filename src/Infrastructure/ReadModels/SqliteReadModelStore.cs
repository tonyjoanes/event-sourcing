using System.Data;
using Dapper;
using Domain.ValueObjects;
using Infrastructure.ReadModels.Projections;
using Microsoft.Data.Sqlite;

namespace Infrastructure.ReadModels;

public class SqliteReadModelStore : IReadModelStore
{
    private readonly string _connectionString;

    public SqliteReadModelStore(string connectionString)
    {
        _connectionString = connectionString;

        // Configure Dapper to handle DateTimeOffset conversion from SQLite strings
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new NullableDateTimeOffsetHandler());
    }

    private class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            if (value is string stringValue)
            {
                return DateTimeOffset.Parse(stringValue);
            }
            return (DateTimeOffset)value;
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.ToString("O"); // ISO 8601 format
        }
    }

    private class NullableDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
    {
        public override DateTimeOffset? Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is string stringValue)
            {
                return DateTimeOffset.Parse(stringValue);
            }
            return (DateTimeOffset)value;
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
        {
            parameter.Value = value?.ToString("O") ?? (object)DBNull.Value;
        }
    }

    private IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public async Task<T?> GetByIdAsync<T>(string id)
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var sql = $"SELECT * FROM {tableName} WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>()
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var sql = $"SELECT * FROM {tableName}";
        return await connection.QueryAsync<T>(sql);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        where T : class
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(sql, parameters);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<T> InsertAsync<T>(T entity)
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var properties = GetProperties<T>();
        var columns = string.Join(", ", properties);
        var values = string.Join(", ", properties.Select(p => "@" + p));
        var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

        await connection.ExecuteAsync(sql, entity);
        return entity;
    }

    public async Task<T> UpdateAsync<T>(T entity)
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var properties = GetProperties<T>().Where(p => p != "Id");
        var setClause = string.Join(", ", properties.Select(p => $"{p} = @{p}"));
        var sql = $"UPDATE {tableName} SET {setClause} WHERE Id = @Id";

        await connection.ExecuteAsync(sql, entity);
        return entity;
    }

    public async Task<bool> DeleteAsync<T>(string id)
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var sql = $"DELETE FROM {tableName} WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync<T>(string id)
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var sql = $"SELECT COUNT(1) FROM {tableName} WHERE Id = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<int> CountAsync<T>()
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var sql = $"SELECT COUNT(1) FROM {tableName}";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<IEnumerable<T>> GetByAccountIdAsync<T>(AccountId accountId)
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var sql = $"SELECT * FROM {tableName} WHERE AccountId = @AccountId";
        return await connection.QueryAsync<T>(sql, new { AccountId = accountId.Value });
    }

    public async Task<IEnumerable<T>> GetByCustomerIdAsync<T>(CustomerId customerId)
        where T : class
    {
        using var connection = CreateConnection();
        var tableName = GetTableName<T>();
        var sql = $"SELECT * FROM {tableName} WHERE CustomerId = @CustomerId";
        return await connection.QueryAsync<T>(sql, new { CustomerId = customerId.Value });
    }

    public async Task<AccountSummaryProjection?> GetAccountSummaryAsync(string accountId)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM AccountSummaryProjection WHERE Id = @AccountId";
        return await connection.QueryFirstOrDefaultAsync<AccountSummaryProjection>(
            sql,
            new { AccountId = accountId }
        );
    }

    public async Task<List<TransactionHistoryProjection>> GetTransactionHistoryAsync(
        string accountId,
        int limit = 50,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null
    )
    {
        using var connection = CreateConnection();

        var sql = "SELECT * FROM TransactionHistoryProjection WHERE AccountId = @AccountId";
        var parameters = new Dictionary<string, object> { { "AccountId", accountId } };

        if (fromDate.HasValue)
        {
            sql += " AND Timestamp >= @FromDate";
            parameters["FromDate"] = fromDate.Value;
        }

        if (toDate.HasValue)
        {
            sql += " AND Timestamp <= @ToDate";
            parameters["ToDate"] = toDate.Value;
        }

        sql += " ORDER BY Timestamp DESC LIMIT @Limit";
        parameters["Limit"] = limit;

        var transactions = await connection.QueryAsync<TransactionHistoryProjection>(
            sql,
            parameters
        );
        return transactions.ToList();
    }

    private static string GetTableName<T>()
    {
        var type = typeof(T);
        return type.Name;
    }

    private static IEnumerable<string> GetProperties<T>()
    {
        var type = typeof(T);
        return type.GetProperties().Select(p => p.Name);
    }
}

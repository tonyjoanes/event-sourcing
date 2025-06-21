using Dapper;
using Microsoft.Data.Sqlite;

namespace Infrastructure.ReadModels;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Create AccountSummary table
        var createAccountSummaryTable = @"
            CREATE TABLE IF NOT EXISTS AccountSummaryProjection (
                Id TEXT PRIMARY KEY,
                CustomerId TEXT NOT NULL,
                Balance REAL NOT NULL,
                Currency TEXT NOT NULL,
                Status TEXT NOT NULL,
                LastTransactionAt TEXT,
                OpenedAt TEXT NOT NULL,
                ClosedAt TEXT,
                OverdraftLimit REAL NOT NULL,
                DailyLimit REAL NOT NULL,
                MinimumBalance REAL NOT NULL
            )";

        await connection.ExecuteAsync(createAccountSummaryTable);

        // Create TransactionHistory table
        var createTransactionHistoryTable = @"
            CREATE TABLE IF NOT EXISTS TransactionHistoryProjection (
                Id TEXT PRIMARY KEY,
                AccountId TEXT NOT NULL,
                Type TEXT NOT NULL,
                Amount REAL NOT NULL,
                Currency TEXT NOT NULL,
                Description TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                RelatedAccountId TEXT,
                CustomerId TEXT NOT NULL
            )";

        await connection.ExecuteAsync(createTransactionHistoryTable);

        // Create indexes for better query performance
        var createIndexes = @"
            CREATE INDEX IF NOT EXISTS IX_TransactionHistory_AccountId ON TransactionHistoryProjection(AccountId);
            CREATE INDEX IF NOT EXISTS IX_TransactionHistory_Timestamp ON TransactionHistoryProjection(Timestamp);
            CREATE INDEX IF NOT EXISTS IX_TransactionHistory_Type ON TransactionHistoryProjection(Type);
            CREATE INDEX IF NOT EXISTS IX_AccountSummary_CustomerId ON AccountSummaryProjection(CustomerId);
            CREATE INDEX IF NOT EXISTS IX_AccountSummary_Status ON AccountSummaryProjection(Status);
        ";

        await connection.ExecuteAsync(createIndexes);
    }
} 
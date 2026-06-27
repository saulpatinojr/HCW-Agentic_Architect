using Microsoft.Data.Sqlite;

namespace WorkspaceManager.Services;

public sealed class ContextOptimizationHistoryStore
{
    private readonly string _dbPath;

    public ContextOptimizationHistoryStore()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string dataDir = Path.Combine(root, "AIArchitectAgents", "data");
        Directory.CreateDirectory(dataDir);
        _dbPath = Path.Combine(dataDir, "context-optimization.db");
        EnsureDatabase();
    }

    public void SaveSnapshot(CompressionSnapshot snapshot)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        using var insertSnapshot = connection.CreateCommand();
        insertSnapshot.CommandText =
            "INSERT INTO snapshots(ts_utc, requests_total, tokens_saved, savings_percent, overhead_ms, output_tokens_saved, estimated_cost_saved_usd) " +
            "VALUES($ts, $req, $saved, $pct, $over, $out, $cost);";
        insertSnapshot.Parameters.AddWithValue("$ts", snapshot.Timestamp.ToUniversalTime().ToString("O"));
        insertSnapshot.Parameters.AddWithValue("$req", snapshot.RequestsTotal);
        insertSnapshot.Parameters.AddWithValue("$saved", snapshot.TokensSaved);
        insertSnapshot.Parameters.AddWithValue("$pct", snapshot.SavingsPercent);
        insertSnapshot.Parameters.AddWithValue("$over", snapshot.OverheadMsAverage);
        insertSnapshot.Parameters.AddWithValue("$out", snapshot.OutputTokensSaved);
        insertSnapshot.Parameters.AddWithValue("$cost", snapshot.EstimatedCostSavedUsd);
        insertSnapshot.ExecuteNonQuery();

        long snapshotId = GetLastInsertRowId(connection);
        foreach (var partner in snapshot.PartnerSavings)
        {
            using var insertPartner = connection.CreateCommand();
            insertPartner.CommandText =
                "INSERT INTO partner_savings(snapshot_id, partner, tokens_saved, percent_total) " +
                "VALUES($sid, $partner, $saved, $pct);";
            insertPartner.Parameters.AddWithValue("$sid", snapshotId);
            insertPartner.Parameters.AddWithValue("$partner", partner.Partner);
            insertPartner.Parameters.AddWithValue("$saved", partner.TokensSaved);
            insertPartner.Parameters.AddWithValue("$pct", partner.PercentOfTotal);
            insertPartner.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static long GetLastInsertRowId(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT last_insert_rowid();";
        return (long)(command.ExecuteScalar() ?? 0L);
    }

    public IReadOnlyList<CompressionTrendPoint> GetDayTrend(int limit = 14)
    {
        const string sql =
            "SELECT substr(ts_utc, 1, 10) AS bucket, MAX(tokens_saved) AS value " +
            "FROM snapshots GROUP BY bucket ORDER BY bucket DESC LIMIT $limit;";
        return ReadTrend(sql, limit, reverseOrder: true);
    }

    public IReadOnlyList<CompressionTrendPoint> GetWeekTrend(int limit = 12)
    {
        const string sql =
            "SELECT strftime('%Y-W%W', ts_utc) AS bucket, MAX(tokens_saved) AS value " +
            "FROM snapshots GROUP BY bucket ORDER BY bucket DESC LIMIT $limit;";
        return ReadTrend(sql, limit, reverseOrder: true);
    }

    public IReadOnlyList<CompressionTrendPoint> GetMonthTrend(int limit = 12)
    {
        const string sql =
            "SELECT substr(ts_utc, 1, 7) AS bucket, MAX(tokens_saved) AS value " +
            "FROM snapshots GROUP BY bucket ORDER BY bucket DESC LIMIT $limit;";
        return ReadTrend(sql, limit, reverseOrder: true);
    }

    private IReadOnlyList<CompressionTrendPoint> ReadTrend(string sql, int limit, bool reverseOrder)
    {
        var items = new List<CompressionTrendPoint>();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$limit", limit);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new CompressionTrendPoint
            {
                Label = reader.IsDBNull(0) ? "n/a" : reader.GetString(0),
                TokensSaved = reader.IsDBNull(1) ? 0 : reader.GetInt32(1)
            });
        }

        if (reverseOrder)
        {
            items.Reverse();
        }

        return items;
    }

    private void EnsureDatabase()
    {
        using var connection = OpenConnection();

        using var createSnapshots = connection.CreateCommand();
        createSnapshots.CommandText =
            "CREATE TABLE IF NOT EXISTS snapshots(" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "ts_utc TEXT NOT NULL, " +
            "requests_total INTEGER NOT NULL, " +
            "tokens_saved INTEGER NOT NULL, " +
            "savings_percent REAL NOT NULL, " +
            "overhead_ms REAL NOT NULL, " +
            "output_tokens_saved INTEGER NOT NULL, " +
            "estimated_cost_saved_usd REAL NOT NULL" +
            ");";
        createSnapshots.ExecuteNonQuery();

        using var createPartner = connection.CreateCommand();
        createPartner.CommandText =
            "CREATE TABLE IF NOT EXISTS partner_savings(" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "snapshot_id INTEGER NOT NULL, " +
            "partner TEXT NOT NULL, " +
            "tokens_saved INTEGER NOT NULL, " +
            "percent_total REAL NOT NULL, " +
            "FOREIGN KEY(snapshot_id) REFERENCES snapshots(id)" +
            ");";
        createPartner.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        return connection;
    }
}

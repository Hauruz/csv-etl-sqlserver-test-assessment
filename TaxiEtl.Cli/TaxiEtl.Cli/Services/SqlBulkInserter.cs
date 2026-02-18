using Microsoft.Data.SqlClient;
using System.Data;
using TaxiEtl.Cli.Models;

namespace TaxiEtl.Cli.Services;

public sealed class SqlBulkInserter
{
    private readonly string _connectionString;
    private readonly string _tableName;

    public SqlBulkInserter(string connectionString, string tableName)
    {
        _connectionString = connectionString;
        _tableName = tableName;
    }

    public async Task InsertAsync(IEnumerable<TripRecord> records, int batchSize, CancellationToken ct = default)
    {
        using var table = CreateTable();

        foreach (var r in records)
        {
            var row = table.NewRow();
            row["pickup_utc"] = r.PickupUtc;
            row["dropoff_utc"] = r.DropoffUtc;
            row["passenger_count"] = r.PassengerCount;
            row["trip_distance"] = r.TripDistance;
            row["store_and_fwd_flag"] = r.StoreAndFwdFlag;
            row["PULocationID"] = r.PULocationID;
            row["DOLocationID"] = r.DOLocationID;
            row["fare_amount"] = r.FareAmount;
            row["tip_amount"] = r.TipAmount;
            table.Rows.Add(row);

            if (table.Rows.Count >= batchSize)
            {
                await FlushAsync(table, ct);
                table.Clear();
            }
        }

        if (table.Rows.Count > 0)
        {
            await FlushAsync(table, ct);
            table.Clear();
        }
    }

    private async Task FlushAsync(DataTable table, CancellationToken ct)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        using var bulk = new SqlBulkCopy(conn)
        {
            DestinationTableName = _tableName,
            BatchSize = table.Rows.Count,
            BulkCopyTimeout = 0
        };

        bulk.ColumnMappings.Add("pickup_utc", "pickup_utc");
        bulk.ColumnMappings.Add("dropoff_utc", "dropoff_utc");
        bulk.ColumnMappings.Add("passenger_count", "passenger_count");
        bulk.ColumnMappings.Add("trip_distance", "trip_distance");
        bulk.ColumnMappings.Add("store_and_fwd_flag", "store_and_fwd_flag");
        bulk.ColumnMappings.Add("PULocationID", "PULocationID");
        bulk.ColumnMappings.Add("DOLocationID", "DOLocationID");
        bulk.ColumnMappings.Add("fare_amount", "fare_amount");
        bulk.ColumnMappings.Add("tip_amount", "tip_amount");

        await bulk.WriteToServerAsync(table, ct);
    }

    private static DataTable CreateTable()
    {
        var t = new DataTable();
        t.Columns.Add("pickup_utc", typeof(DateTime));
        t.Columns.Add("dropoff_utc", typeof(DateTime));
        t.Columns.Add("passenger_count", typeof(short));
        t.Columns.Add("trip_distance", typeof(decimal));
        t.Columns.Add("store_and_fwd_flag", typeof(string));
        t.Columns.Add("PULocationID", typeof(int));
        t.Columns.Add("DOLocationID", typeof(int));
        t.Columns.Add("fare_amount", typeof(decimal));
        t.Columns.Add("tip_amount", typeof(decimal));
        return t;
    }
}
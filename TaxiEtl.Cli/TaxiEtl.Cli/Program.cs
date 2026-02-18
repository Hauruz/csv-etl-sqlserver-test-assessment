using System.Diagnostics;
using Microsoft.Data.SqlClient;
using TaxiEtl.Cli.Services;

const int BatchSize = 10_000;

var csvPath = args.Length > 0 ? args[0] : "sample-cab-data.csv";
var connectionString = Environment.GetEnvironmentVariable("TAXIDB_CONNECTION");
var duplicatesPath = "duplicates.csv";
var tableName = "dbo.TripRecords";

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Error: set TAXIDB_CONNECTION environment variable.");
    Console.Error.WriteLine("Example: Server=localhost,1433;Database=TaxiDb;User Id=sa;Password=***;TrustServerCertificate=True;");
    return;
}

Console.WriteLine($"CSV  : {csvPath}");
Console.WriteLine($"DB   : {new SqlConnectionStringBuilder(connectionString) { Password = "***" }}");
Console.WriteLine();

var sw = Stopwatch.StartNew();

var reader = new CsvTripReader();
var transformer = new TripTransformer();
var inserter = new SqlBulkInserter(connectionString, tableName);

long totalRead = 0;
long totalInserted = 0;
long skipped = 0;

using (var dedup = new DuplicateDetector(duplicatesPath))
{
    var transformed = reader.ReadRows(csvPath)
        .Select(row =>
        {
            Interlocked.Increment(ref totalRead);
            return row;
        })
        .Select(row => transformer.TryTransform(row, out var rec) ? rec : null!)
        .Where(rec =>
        {
            if (rec is null) { Interlocked.Increment(ref skipped); return false; }
            return true;
        })
        .Where(rec => !dedup.IsDuplicate(rec));

    await inserter.InsertAsync(transformed, BatchSize);

    totalInserted = totalRead - skipped - dedup.DuplicatesCount;

    Console.WriteLine();
    Console.WriteLine($"Rows read       : {totalRead:N0}");
    Console.WriteLine($"Rows skipped    : {skipped:N0}  (bad / unparseable)");
    Console.WriteLine($"Duplicates      : {dedup.DuplicatesCount:N0}");
    Console.WriteLine($"Rows inserted   : {totalInserted:N0}");
}

sw.Stop();
Console.WriteLine($"Elapsed         : {sw.Elapsed}");
Console.WriteLine("Done.");

using TaxiEtl.Cli.Models;
using System.Text;

namespace TaxiEtl.Cli.Services;

public sealed class DuplicateDetector : IDisposable
{
    private readonly HashSet<DuplicateKey> _seen = new();
    private readonly StreamWriter _writer;

    public long DuplicatesCount { get; private set; }

    public DuplicateDetector(string duplicatesPath)
    {
        _writer = new StreamWriter(duplicatesPath, false, Encoding.UTF8);
        _writer.WriteLine("pickup_utc,dropoff_utc,passenger_count,trip_distance,store_and_fwd_flag,PULocationID,DOLocationID,fare_amount,tip_amount");
    }

    public bool IsDuplicate(TripRecord record)
    {
        var key = new DuplicateKey(record.PickupUtc, record.DropoffUtc, record.PassengerCount);

        if (!_seen.Add(key))
        {
            _writer.WriteLine(string.Join(",",
                record.PickupUtc.ToString("o"),
                record.DropoffUtc.ToString("o"),
                record.PassengerCount,
                record.TripDistance,
                record.StoreAndFwdFlag,
                record.PULocationID,
                record.DOLocationID,
                record.FareAmount,
                record.TipAmount));
            DuplicatesCount++;
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        _writer.Flush();
        _writer.Dispose();
    }
}
using TaxiEtl.Cli.Models;
using System.Globalization;

namespace TaxiEtl.Cli.Services;

public sealed class TripTransformer
{
    private static readonly TimeZoneInfo Est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

    public bool TryTransform(IDictionary<string, string> row, out TripRecord record)
    {
        record = default!;

        try
        {
            if (!DateTime.TryParse(row["tpep_pickup_datetime"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var pickupLocal))
                return false;

            if (!DateTime.TryParse(row["tpep_dropoff_datetime"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dropoffLocal))
                return false;

            if (!short.TryParse(row["passenger_count"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var passengerCount))
                return false;

            if (!decimal.TryParse(row["trip_distance"], NumberStyles.Number, CultureInfo.InvariantCulture, out var tripDistance))
                return false;

            if (!int.TryParse(row["PULocationID"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pu))
                return false;

            if (!int.TryParse(row["DOLocationID"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var dl))
                return false;

            if (!decimal.TryParse(row["fare_amount"], NumberStyles.Number, CultureInfo.InvariantCulture, out var fare))
                return false;

            if (!decimal.TryParse(row["tip_amount"], NumberStyles.Number, CultureInfo.InvariantCulture, out var tip))
                return false;

            var flagRaw = row["store_and_fwd_flag"].Trim();
            var flag = flagRaw.Equals("Y", StringComparison.OrdinalIgnoreCase) ? "Yes"
                     : flagRaw.Equals("N", StringComparison.OrdinalIgnoreCase) ? "No"
                     : flagRaw;

            var pickupUtc = TimeZoneInfo.ConvertTimeToUtc(pickupLocal, Est);
            var dropoffUtc = TimeZoneInfo.ConvertTimeToUtc(dropoffLocal, Est);

            record = new TripRecord
            {
                PickupUtc = pickupUtc,
                DropoffUtc = dropoffUtc,
                PassengerCount = passengerCount,
                TripDistance = tripDistance,
                StoreAndFwdFlag = flag,
                PULocationID = pu,
                DOLocationID = dl,
                FareAmount = fare,
                TipAmount = tip
            };

            return true;
        }
        catch
        {
            return false;
        }
    }
}
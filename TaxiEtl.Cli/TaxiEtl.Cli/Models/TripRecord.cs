namespace TaxiEtl.Cli.Models;

public sealed class TripRecord
{
    public DateTime PickupUtc { get; set; }
    public DateTime DropoffUtc { get; set; }
    public short PassengerCount { get; set; }
    public decimal TripDistance { get; set; }
    public string StoreAndFwdFlag { get; set; } = string.Empty;
    public int PULocationID { get; set; }
    public int DOLocationID { get; set; }
    public decimal FareAmount { get; set; }
    public decimal TipAmount { get; set; }
}

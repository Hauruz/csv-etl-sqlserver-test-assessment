namespace TaxiEtl.Cli.Models;

public readonly record struct DuplicateKey(DateTime PickupUtc, DateTime DropoffUtc, short PassengerCount);
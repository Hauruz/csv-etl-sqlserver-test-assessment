using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace TaxiEtl.Cli.Services;

public sealed class CsvTripReader
{
    public IEnumerable<IDictionary<string, string>> ReadRows(string path)
    {
        using var reader = new StreamReader(path);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
        };

        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var row = csv.HeaderRecord!
                .ToDictionary(h => h, h => csv.GetField(h) ?? string.Empty);

            yield return row;
        }
    }
}
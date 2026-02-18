# TaxiEtl.Cli

Simple ETL tool that reads yellow-taxi trip records from a CSV and loads them into SQL Server.

## How to run

You'll need .NET 8 SDK and Docker.

```bash
# copy .env.example to .env and set your password
cp .env.example .env

# spin up SQL Server
docker compose up -d

# give it a few seconds to start, then init the DB
sqlcmd -S "localhost,1433" -U sa -P "<your_password>" -i TaxiEtl.Cli/TaxiEtl.Cli/Sql/001_CreateDatabase.sql

# drop sample-cab-data.csv into TaxiEtl.Cli/TaxiEtl.Cli/ and run
cd TaxiEtl.Cli/TaxiEtl.Cli
set TAXIDB_CONNECTION=Server=localhost,1433;Database=TaxiDb;User Id=sa;Password=<your_password>;TrustServerCertificate=True;
dotnet run
```

CSV path can also be passed as an argument: `dotnet run -- "other-file.csv"`.

## Numbers

After running against the provided dataset:

- 30 000 rows in the CSV
- 49 couldn't be parsed (bad data)
- 111 duplicates removed → saved to `duplicates.csv`
- **29 840 rows inserted into the DB**

## What I'd change for a 10 GB file

The current code already streams rows one by one (no full file in memory), but at 10 GB scale there's more to think about:

- **Parallel read/transform/insert** — split the pipeline into stages with `Channels` or `TPL Dataflow` so reading, transforming and inserting happen concurrently.
- **Duplicate detection won't fit in RAM** — swap `HashSet` for something disk-backed (LevelDB, RocksDB) or partition the file by dedup-key hash and process chunks independently.
- **Faster bulk insert** — use `SqlBulkCopyOptions.TableLock` for minimal logging, drop indexes before load and recreate after, possibly run 2-4 parallel insert streams.
- **Tune batch size** — bump from 10k to 50-100k rows per batch and benchmark on the target hardware.

## Assumptions

- CSV format matches the provided sample (same headers).
- Dates are in US Eastern Time → converted to UTC on import.
- Duplicate = same `(pickup_datetime, dropoff_datetime, passenger_count)`. First one wins, the rest go to `duplicates.csv`.
- Unparseable rows are skipped silently (counted in output).
- `store_and_fwd_flag` is either `Y`/`N` — mapped to `Yes`/`No`. Anything else stays as-is.

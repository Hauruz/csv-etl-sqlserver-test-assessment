IF DB_ID('TaxiDb') IS NULL
    CREATE DATABASE TaxiDb;
GO

USE TaxiDb;
GO

IF OBJECT_ID('dbo.TripRecords', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TripRecords
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL,
        pickup_utc          DATETIME2(0)         NOT NULL,
        dropoff_utc         DATETIME2(0)         NOT NULL,
        passenger_count     SMALLINT             NOT NULL,
        trip_distance       DECIMAL(10, 2)       NOT NULL,
        store_and_fwd_flag  VARCHAR(3)           NOT NULL,
        PULocationID        INT                  NOT NULL,
        DOLocationID        INT                  NOT NULL,
        fare_amount         DECIMAL(10, 2)       NOT NULL,
        tip_amount          DECIMAL(10, 2)       NOT NULL,

        CONSTRAINT PK_TripRecords PRIMARY KEY CLUSTERED (Id)
    );
END
GO

-- avg tip per pickup location
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TripRecords_PULocationID_TipAmount' AND object_id = OBJECT_ID('dbo.TripRecords'))
    CREATE NONCLUSTERED INDEX IX_TripRecords_PULocationID_TipAmount
        ON dbo.TripRecords (PULocationID)
        INCLUDE (tip_amount);
GO

-- top 100 by distance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TripRecords_TripDistance_Desc' AND object_id = OBJECT_ID('dbo.TripRecords'))
    CREATE NONCLUSTERED INDEX IX_TripRecords_TripDistance_Desc
        ON dbo.TripRecords (trip_distance DESC);
GO

-- top 100 by trip duration
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TripRecords_Pickup_Dropoff' AND object_id = OBJECT_ID('dbo.TripRecords'))
    CREATE NONCLUSTERED INDEX IX_TripRecords_Pickup_Dropoff
        ON dbo.TripRecords (pickup_utc, dropoff_utc);
GO

-- general queries filtering by PULocationID (covering)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TripRecords_PULocationID' AND object_id = OBJECT_ID('dbo.TripRecords'))
    CREATE NONCLUSTERED INDEX IX_TripRecords_PULocationID
        ON dbo.TripRecords (PULocationID)
        INCLUDE (pickup_utc, dropoff_utc, passenger_count, trip_distance,
                 store_and_fwd_flag, DOLocationID, fare_amount, tip_amount);
GO

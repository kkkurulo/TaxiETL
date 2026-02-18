CREATE DATABASE Trips;
USE Trips;

CREATE TABLE Trips (
    Id INT IDENTITY(1,1) PRIMARY KEY, 

    tpep_pickup_datetime DATETIME2 NOT NULL,
    tpep_dropoff_datetime DATETIME2 NOT NULL, 
    passenger_count INT NOT NULL,
    trip_distance DECIMAL(10, 2) NOT NULL,
    store_and_fwd_flag VARCHAR(3) NOT NULL, 
    PULocationID INT NOT NULL,
    DOLocationID INT NOT NULL,
    fare_amount DECIMAL(10, 2) NOT NULL,
    tip_amount DECIMAL(10, 2) NOT NULL,

    trip_duration_seconds AS DATEDIFF(second, tpep_pickup_datetime, tpep_dropoff_datetime) PERSISTED
);

SELECT * FROM Trips;

 -- Create index. Includes tip_amount to speed up reads.
CREATE NONCLUSTERED INDEX IX_Trips_PULocationID_Include_Tip
ON dbo.Trips (PULocationID)
INCLUDE (tip_amount);

SELECT TOP 10
    PULocationID,
    AVG(tip_amount) as AvgTip
FROM dbo.Trips
GROUP BY PULocationID
ORDER BY AvgTip DESC;

-- Index sorted by distance.
CREATE NONCLUSTERED INDEX IX_Trips_TripDistance
ON dbo.Trips (trip_distance DESC);

SELECT TOP 100
    trip_distance, PULocationID, DOLocationID
FROM dbo.Trips
ORDER BY trip_distance DESC;

-- Index on the computed duration column.
CREATE NONCLUSTERED INDEX IX_Trips_Duration
ON dbo.Trips (trip_duration_seconds DESC);

SELECT TOP 100
    trip_duration_seconds, tpep_pickup_datetime, tpep_dropoff_datetime
FROM dbo.Trips
ORDER BY trip_duration_seconds DESC;

-- Example of search, where part of the condition is PULocationID. 
SELECT * FROM dbo.Trips 
WHERE PULocationID = 100 AND trip_distance > 10; 

-- Checking for duplicate. Should return 0 rows.
SELECT 
    tpep_pickup_datetime, 
    tpep_dropoff_datetime, 
    passenger_count, 
    COUNT(*) as Count
FROM dbo.Trips
GROUP BY 
    tpep_pickup_datetime, 
    tpep_dropoff_datetime, 
    passenger_count
HAVING COUNT(*) > 1;
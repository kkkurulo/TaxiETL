using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace TaxiETL.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    
    /// <summary>
    /// Initializes a new instance of the DatabaseService with the specified connection string.
    /// </summary>
    /// <param name="connectionString"></param>
    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Performs a bulk insert of trip records into the dbo.Trips table.
    /// </summary>
    /// <param name="records"></param>
    public void BulkInsert(List<TripRecord> records)
    {
        var table = CreateDataTable();

        foreach (var r in records)
        {
            DateTime pickupEst = DateTime.ParseExact(r.PickupString, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            DateTime dropoffEst = DateTime.ParseExact(r.DropoffString, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            table.Rows.Add(
                TimeZoneInfo.ConvertTimeToUtc(pickupEst, est),
                TimeZoneInfo.ConvertTimeToUtc(dropoffEst, est),
                r.PassengerCount ?? 0,
                r.TripDistance,
                r.StoreAndFwdFlag == "Y" ? "Yes" : "No",
                r.PULocationID,
                r.DOLocationID,
                r.FareAmount,
                r.TipAmount
            );
        }

        using var bulk = new SqlBulkCopy(_connectionString);
        bulk.DestinationTableName = "dbo.Trips";

        MapColumns(bulk);

        try
        {
            bulk.WriteToServer(table);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates a DataTable with the schema matching the dbo.Trips table.
    /// </summary>
    /// <returns></returns>
    private DataTable CreateDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("tpep_pickup_datetime", typeof(DateTime));
        table.Columns.Add("tpep_dropoff_datetime", typeof(DateTime));
        table.Columns.Add("passenger_count", typeof(int));
        table.Columns.Add("trip_distance", typeof(decimal));
        table.Columns.Add("store_and_fwd_flag", typeof(string));
        table.Columns.Add("PULocationID", typeof(int));
        table.Columns.Add("DOLocationID", typeof(int));
        table.Columns.Add("fare_amount", typeof(decimal));
        table.Columns.Add("tip_amount", typeof(decimal));
        return table;
    }


    /// <summary>
    /// Truncates the dbo.Trips table, removing all records.
    /// </summary>
    public void TruncateTable()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand("TRUNCATE TABLE dbo.Trips", connection);
        command.ExecuteNonQuery();
        Console.WriteLine("Table has been cleared.");
    }


    /// <summary>
    /// Maps DataTable columns to SQL table columns for SqlBulkCopy.
    /// </summary>
    /// <param name="bulk"></param>
    private void MapColumns(SqlBulkCopy bulk)
    {
        bulk.ColumnMappings.Add("tpep_pickup_datetime", "tpep_pickup_datetime");
        bulk.ColumnMappings.Add("tpep_dropoff_datetime", "tpep_dropoff_datetime");
        bulk.ColumnMappings.Add("passenger_count", "passenger_count");
        bulk.ColumnMappings.Add("trip_distance", "trip_distance");
        bulk.ColumnMappings.Add("store_and_fwd_flag", "store_and_fwd_flag");
        bulk.ColumnMappings.Add("PULocationID", "PULocationID");
        bulk.ColumnMappings.Add("DOLocationID", "DOLocationID");
        bulk.ColumnMappings.Add("fare_amount", "fare_amount");
        bulk.ColumnMappings.Add("tip_amount", "tip_amount");
    }
}

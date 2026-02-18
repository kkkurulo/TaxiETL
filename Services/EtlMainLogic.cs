using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper;
using TaxiETL.Constants;

namespace TaxiETL.Services;

public class EtlMainLogic
{
    private readonly DatabaseService _dbService;

    public EtlMainLogic(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    /// <summary>
    /// Runs the ETL process: reads records from the input CSV, filters duplicates, writes duplicates to a file, and inserts unique records into the database in batches.
    /// </summary>
    /// <param name="inputFile"></param>
    /// <param name="duplicatesFile"></param>
  
    public void Run(string inputFile, string duplicatesFile)
    {
        Console.WriteLine("Logic started");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var uniqueKeys = new HashSet<string>();
        var batch = new List<TripRecord>(Config.BatchSize);
        int totalProcessed = 0;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            MissingFieldFound = null
        };

        using var reader = new StreamReader(inputFile);
        using var csvReader = new CsvReader(reader, config);

        using var writer = new StreamWriter(duplicatesFile);
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csvWriter.WriteHeader<TripRecord>();
        csvWriter.NextRecord();

        var records = csvReader.GetRecords<TripRecord>();

        foreach (var record in records)
        {
            record.StoreAndFwdFlag = record.StoreAndFwdFlag?.Trim();
            record.PickupString = record.PickupString?.Trim();
            record.DropoffString = record.DropoffString?.Trim();

            string key = $"{record.PickupString}-{record.DropoffString}-{record.PassengerCount}";

            if (uniqueKeys.Contains(key))
            {
                csvWriter.WriteRecord(record);
                csvWriter.NextRecord();
            }
            else
            {
                uniqueKeys.Add(key);
                batch.Add(record);
                totalProcessed++;

                if (batch.Count >= Config.BatchSize)
                {
                    _dbService.BulkInsert(batch);
                    batch.Clear();
                    Console.Write(".");
                }
            }
        }

        if (batch.Count > 0)
        {
            _dbService.BulkInsert(batch);
        }

        stopwatch.Stop();
        Console.WriteLine($"\nFinish");
        Console.WriteLine($"Total Unique Records: {totalProcessed}");
        Console.WriteLine($"Time Elapsed: {stopwatch.Elapsed.TotalSeconds:N2} sec");
    }
}

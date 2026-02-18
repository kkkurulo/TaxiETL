
namespace TaxiETL.Constants;

public static class Config
{
    public const string ConnectionString = "Server=Victus-Kyrylo;Database=Trips;Trusted_Connection=True;TrustServerCertificate=True;";
    public const string InputFile = @"C:\Users\kyryl\source\repos\TaxiETL\sample-cab-data.csv";
    public const string DuplicatesFile = @"C:\Users\kyryl\source\repos\TaxiETL\duplicates.csv";
    public const int BatchSize = 10000; // value that represents a buffer size for insert
}

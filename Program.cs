using TaxiETL.Constants;
using TaxiETL.Services;

namespace TaxiETL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var dbService = new DatabaseService(Config.ConnectionString);
                var pipeline = new EtlMainLogic(dbService);
                dbService.TruncateTable();
                pipeline.Run(Config.InputFile, Config.DuplicatesFile);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Error: {ex.Message}");
            }
        }
    }
}

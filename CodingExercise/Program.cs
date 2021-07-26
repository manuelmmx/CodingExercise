using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodingExercise
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                string fileUrl = configuration["FileURL"];
                string filePath = configuration["FilePath"];
                string taxUrl = configuration["TaxUrl"];

                if (string.IsNullOrEmpty(fileUrl) 
                        || string.IsNullOrEmpty(filePath)
                            || string.IsNullOrEmpty(taxUrl))
                {
                    Console.WriteLine("Some or all of file url, file path or tax url configuration settings are missing. Please add them and try again.");
                    return;
                }

                var propManager = new PropertyManager(fileUrl, filePath, taxUrl);

                //Use a utilit file to encapsulate File object use

                var allRecords = await propManager.GetRawRecords();

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("******************************** Raw Records Display *********************************************************");
                Console.WriteLine(Environment.NewLine);
                DisplayRowsRecordsOnScreen(allRecords);

                var allProperties = await propManager.GetPropertiesFromRawRecords();
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("******************************** Property objects Display *********************************************************");
                Console.WriteLine(Environment.NewLine);
                DisplayObjectsOnScreen(allProperties);

                string propertyType = "Residential";
                HashSet<Property> biggestPropertiesList = await propManager.GetBiggestFlatsPerCityPerType(propertyType);
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("******************************** Biggest Residential flats per city *********************************************************");
                Console.WriteLine(Environment.NewLine);
                DisplayObjectsOnScreen(biggestPropertiesList);

                var biggestCheaper = await propManager.GetCheapestBiggestFlat();
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("******************************** Cheapest with the most rooms flats *********************************************************");
                Console.WriteLine(Environment.NewLine);
                DisplayOneObjectOnScreen(biggestCheaper);

                var mostExpensive = await propManager.GetMostExpensiveOnTotalPriceFlatsPerCity();
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("******************************** More expensive on total price per city *********************************************************");
                Console.WriteLine(Environment.NewLine);
                DisplayObjectsOnScreen(mostExpensive);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong! Please check details \n {ex}");
            }
        }

        private static void DisplayRowsRecordsOnScreen(List<string> allRecords)
        {
            foreach (string line in allRecords)
            {
                string[] columns = line.Split(',');
                var rowData = new StringBuilder();

                foreach (string column in columns)
                {
                    rowData.Append( $" {column} |");
                }

                Console.WriteLine(rowData.ToString());
            }
        }

        private static void DisplayObjectsOnScreen(HashSet<Property> allLocations)
        {
            foreach (Property singleLocation in allLocations)
            {
                DisplayOneObjectOnScreen(singleLocation);
            }
        }
        private static void DisplayOneObjectOnScreen(Property singleProperty)
        {

                Console.WriteLine($"{singleProperty.Street} | {singleProperty.City} | {singleProperty.ZipCode} | {singleProperty.State} | " +
                                    $"{singleProperty.Beds} | {singleProperty.Baths} | {singleProperty.SizeInSqFt:N0} | {singleProperty.Type} |" +
                                        $"{singleProperty.SaleDate:f} | {singleProperty.Price:C} | {singleProperty.Latitude} | {singleProperty.Longitude}");
        }

    }
}

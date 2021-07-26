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
    public class PropertyManager
    {
        private readonly string _fileUrl;
        private readonly string _filePath;
        private readonly string _taxUrl;
        private List<string> _allRecords;
        private HashSet<Property> _allProperties;

        public PropertyManager(string  fileUrl, string filePath, string taxUrl)
        {
            _fileUrl = fileUrl;
            _filePath = filePath;
            _taxUrl = taxUrl;

            if (string.IsNullOrEmpty(_fileUrl)
                || string.IsNullOrEmpty(_filePath)
                    || string.IsNullOrEmpty(_taxUrl)) {
                throw new ArgumentException("You must provide a valid fileUrl, filePath and taxUrl values.");
            }
        }

        /// <summary>
        /// It returns an List<string> which contains raw data from the file that is downloaded from the file Url proviede.
        /// </summary>
        /// <returns>List<string> of row daat List</returns>
        public async Task<List<string>> GetRawRecords()
        {
            try
            {
                //Only get the file if we have not done it yet.
                if (_allRecords is null)
                    await DownloadFileAsync(_fileUrl, _filePath);

                var records = File.ReadAllLines(_filePath);
                _allRecords = new List<string>(records);

                return _allRecords;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when trying to read data from the file on the provided path. Check error details for more information.", ex);
            }            
        }

        /// <summary>
        /// Returns a list of the biggest propertyies per city for a given type.
        /// </summary>
        /// <param name="allLocations">List of All properties</param>
        /// <param name="type">Specfic type to look for</param>
        /// <returns>HashSet<Property></returns>
        public async Task<HashSet<Property>> GetBiggestFlatsPerCityPerType(string type)
        {
            //Get properties if we have not done it yet.
            if (_allProperties is null)
            {
                await GetPropertiesFromRawRecords();
            }

            //Get Largest sizes per city
            var largestPerCity =
                from loc in _allProperties
                where loc.Type.ToLower() == type.ToLower()
                group loc by loc.City into locs
                select new
                {
                    City = locs.Key,
                    SizeInSqFt = locs.Max(locs => locs.SizeInSqFt)
                };

            //Join with whole list to filter out bigger ones
            var finalList = from l in _allProperties
                            join m in largestPerCity on
                            new { l.SizeInSqFt, l.City } equals
                            new { m.SizeInSqFt, m.City }
                            select l;

                return finalList.ToHashSet();
        }


        /// <summary>
        /// Returns a list of the biggest flat in sq ft
        /// </summary>
        /// <param name="allLocations"></param>
        /// <returns>HashSet<Property></returns>
        public async Task<Property> GetCheapestBiggestFlat()
        {
            //Get properties if we have not done it yet.
            if (_allProperties is null)
            {
                await GetPropertiesFromRawRecords();
            }

            //Get Largest sizes per city
            var moreRooms =
                (from loc in _allProperties
                 orderby loc.Price ascending
                 group loc by (loc.Beds + loc.Baths) into locs
                 select new
                 {
                     Rooms = locs.Key,
                     Price = locs.Min(locs => locs.Price)
                 }).FirstOrDefault();

            //Join with whole list to filter out bigger ones
            var finalList = from l in _allProperties
                            where (l.Baths + l.Beds) == moreRooms.Rooms && l.Price == moreRooms.Price
                            select l;

            return finalList.FirstOrDefault();
        }

        /// <summary>
        /// Returns a list of most expensive flats based on total price price + price * tax
        /// </summary>
        /// <param name="allLocations"></param>
        /// <returns>HashSet<Property></returns>
        public async Task<HashSet<Property>> GetMostExpensiveOnTotalPriceFlatsPerCity()
        {
            //Get properties if we have not done it yet.
            if (_allProperties is null)
            {
                await GetPropertiesFromRawRecords();
            }

            //Get Largest sizes per city
            var mostExpensivePerCity =
                from loc in _allProperties
                group loc by loc.City into locs
                select new
                {
                    City = locs.Key,
                    TotalPrice = locs.Max(locs => locs.TotalPrice)
                };

            //Join with whole list to filter out bigger ones
            var finalList = from l in _allProperties
                            join m in mostExpensivePerCity on
                            new { l.TotalPrice, l.City } equals
                            new { m.TotalPrice, m.City }
                            select l;

            return finalList.ToHashSet();
        }

        /// <summary>
        /// Returns an list of Properties that are parsed from the array of comma separated record
        /// </summary>
        /// <returns>Task<HashSet<Property></returns>
        public async Task<HashSet<Property>> GetPropertiesFromRawRecords()
        {
            try
            {
                var locations = new HashSet<Property>();

                //Get the row info if we have not done it yet.
                if (_allRecords is null)
                {
                    await GetRawRecords();
                }

                //for (string line in allRecords)
                foreach (var row in _allRecords.Skip(1))
                {
                    if (_allProperties is null)
                        _allProperties = new HashSet<Property>();

                 var singleLocation = Property.MapFromFileLine(row);
                    _allProperties.Add(singleLocation);
                }

                //We are adding this part here because this tax info will be used lated. 
                var cityWithTaxes = await GetTaxesForUniqueCities(_allProperties, _taxUrl);

                AddTaxesToEachProperty(_allProperties, cityWithTaxes);

                return _allProperties;
            }
            catch (Exception ex)
            {
                throw new Exception ("An error ocurred while parsing raw information. Please see error details.", ex);
            }
        }

        private static async Task<Dictionary<string, decimal>> GetTaxesForUniqueCities(HashSet<Property> locations, string taxUrl)
        {
            try
            {
                HttpClient client = new HttpClient();
                var cities = locations.Select(c => c.City).Distinct().ToList();
                var citiesWithTaxes = new Dictionary<string, decimal>();
                foreach (string city in cities)
                {
                    var stringTask = await client.GetStringAsync($"{taxUrl}{city}");
                    decimal outputTax;
                    citiesWithTaxes.Add(city, decimal.TryParse(stringTask, out outputTax) ? outputTax : 0);
                }

                return citiesWithTaxes;
            }
            catch (Exception ex)
            {
                //SHould be logged
                throw new Exception("An error ocurred while getting taxes for properties.", ex);
            }
        }

        private static void AddTaxesToEachProperty(HashSet<Property> allLocations, Dictionary<string, decimal> taxes)
        {
            foreach (Property location in allLocations)
            {
                decimal tax;
                location.Tax = taxes.TryGetValue(location.City, out tax) ? tax : 0;
            }
        }

        /// <summary>
        /// Download a file from url and save into disk. 
        /// </summary>
        /// <param name="uri">File where to download the file</param>
        /// <param name="outputPath">Local path where the file will be saved.</param>
        /// <returns></returns>
        private static async Task DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();
                Uri uriResult;

                if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
                    throw new InvalidOperationException("URI is invalid.");

                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath); //Get always the latest file.
                }

                byte[] fileBytes = await _httpClient.GetByteArrayAsync(url);
                File.WriteAllBytes(outputPath, fileBytes);
            }
            catch (Exception ex)
            {
                //We should log it. At least we will throw an exception with espeficics.
                throw new Exception("An error ocurred while downloading data file.", ex);
            }
        }
    }
}

using System;
using System.Globalization;

namespace CodingExercise
{
    public class Property
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public int Beds { get; set; }
        public int Baths { get; set; }
        public int SizeInSqFt { get; set; }
        public string Type { get; set; }
        public DateTime SaleDate { get; set; }
        public Decimal Price { get; set; }
        public Decimal TotalPrice => Price + (Price * Tax);
        public Decimal Tax { get; set; }
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }

        public static Property MapFromFileLine(string line)
        {
            string[] location = line.Split(',');
            string formatDate = "ddd MMM dd hh:mm:ss EDT yyyy";

            var houseLocation = new Property();

            houseLocation.Street = location[0]?.Trim();
            houseLocation.City = location[1]?.Trim();
            houseLocation.ZipCode = location[2]?.Trim();
            houseLocation.State = location[3]?.Trim();
            int bedsOut;
            houseLocation.Beds = int.TryParse(location[4], out bedsOut) ? bedsOut : 0;
            int bathsOut;
            houseLocation.Baths = int.TryParse(location[5], out bathsOut) ? bathsOut : 0;
            int sizeOut;
            houseLocation.SizeInSqFt = int.TryParse(location[6], out sizeOut) ? sizeOut : 0;
            houseLocation.Type = location[7]?.Trim();
            houseLocation.SaleDate = DateTime.ParseExact(location[8], formatDate, CultureInfo.InvariantCulture);
            decimal priceOut;
            houseLocation.Price = decimal.TryParse(location[9], out priceOut) ? priceOut : 0;
            double latitudOut;
            houseLocation.Latitude = double.TryParse(location[10], out latitudOut) ? latitudOut : 0;
            double LongitudOut;
            houseLocation.Longitude = double.TryParse(location[11], out LongitudOut) ? LongitudOut : 0;

            return houseLocation;
        }


    }
}

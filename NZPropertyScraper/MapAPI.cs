using Geocoding;
using Geocoding.Extensions;
using Geocoding.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZPropertyScraper
{
    public static class MapAPI
    {
        public static GoogleGeocoder geocoder;
       
        public static async Task<IEnumerable<GoogleAddress>> ValidateSearch(string address)
        {
            geocoder.ComponentFilters = new List<GoogleComponentFilter>();
            geocoder.ComponentFilters.Add(new GoogleComponentFilter("country", "New Zealand"));
            IEnumerable<GoogleAddress> unvaladdresses = await geocoder.GeocodeAsync(address);

            if(unvaladdresses.ToList().Count == 0)
            {
                return null;
            }
            foreach (GoogleAddress addr in unvaladdresses)
            {
                Console.WriteLine(addr.Components[(int)GoogleAddressType.StreetAddress].LongName);
            }
            Console.WriteLine(unvaladdresses.ToList().Count);
            return unvaladdresses;
            
        }
        public static void InitializeClass(string APIKey)
        {
            
            geocoder = new GoogleGeocoder(APIKey);
            
        }
    }
}

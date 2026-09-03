using Geocoding.Google;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace NZPropertyScraper
{
    public static class RealEstateScrape
    {
        public static JsonDocument realestate;
        public static void Initialize()
        {
            realestate = JsonDocument.Parse(File.ReadAllText("JSON/realestate.json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

        }
        public static string GetPath(GoogleAddress address)
        {
            if (realestate == null)
            {
                Initialize();
            }
            List<string> slugs = new List<string>();
            foreach (JsonElement item in realestate.RootElement.GetProperty("included").EnumerateArray())
            {
                if (item.GetProperty("type").GetString() != "suburb")
                    continue;

                JsonElement attributes = item.GetProperty("attributes");

                string slug = attributes.GetProperty("slug").GetString();
                string fqSlug = attributes.GetProperty("fq-slug").GetString();




                slugs.Add(fqSlug);



            }


            HtmlWeb web = new HtmlWeb();
            int key = -1;
            for (int i = 0; i < address.Components.Length; i++)
            {
                if (address.Components[i].Types[0] == GoogleAddressType.Locality)
                {
                    key = i;
                }
            }
            if (key == -1) 
            {
                return null ;
            }
            string locale = address.Components[key].LongName;
            locale = locale.ToLower().Replace(" ","-");
            Console.WriteLine(locale);
            string path = "";
            foreach (string fqslug in slugs)
            {
                if (fqslug.Contains(locale))
                {
                    path = fqslug;
                    break;
                }
            }
            if (path == "")
            {
                return null;
            }
            path = path.Replace("_", "/");
            return path;
        }
        public static string Scrape(GoogleAddress address)
        {
            string html  = ScrapeWebsite(GetPath(address)).GetAwaiter().GetResult();
            _ = GetStatisticalNodes(html);
            return html;
        }
        private static HttpClient httpClient = new HttpClient();
        static async Task<string> ScrapeWebsite(string path)
        {
            string htmlContent = "";
            for (int i = 1; i < 6; i++)
            {
                try
                {
                    htmlContent += await httpClient.GetStringAsync("https://www.realestate.co.nz/residential/sale/" + path + "?page=" + i);
                    Console.WriteLine(i);
                    

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return htmlContent;

                }

            }


            return htmlContent;

        }
        static async Task<string> ScrapeHousePage(string path)
        {
            
            try
            {
                return await httpClient.GetStringAsync("https://www.realestate.co.nz" + path);
            }
            catch
            {
                return null;
                
            }
        }
        public static List<List<string>> GetStatisticalNodes(string html)
        {
            List<List<string>> returnVal = new List<List<string>>();
            returnVal.Add(new List<string>());
            // Get Medians. (Sale Price, Asking Price, Rental Price)
            string selector = ".tile--body";
            IList<HtmlNode> listings =  HTMLOperations.SelectCSS(html, selector);
            foreach (HtmlNode node in listings)
            {
                RealEstateHouse house = new RealEstateHouse(node);
                Console.WriteLine("ListedPrice: " + house.listedPrice + " Bedrooms: " + house.bedrooms + " Bathrooms: " + house.bathrooms + " FloorArea: " + house.floorArea + " LandArea: " + house.landArea);
            }

            return null;
        }
        public class RealEstateHouse {

            public int listedPrice;
            public int bedrooms;
            public int bathrooms;
            public int floorArea;
            public int landArea;

            public RealEstateHouse(HtmlNode listing) {
                //Console.WriteLine(listing.OuterHtml);
                IList<HtmlNode> listingPrice = HTMLOperations.SelectCSS(listing.OuterHtml, "[data-test=\"price-display__price-method\"]");
                if (listingPrice.Count > 0)
                {
                    // Console.WriteLine(node.OuterHtml);
                    // Get Price from node if any
                    Match match = Regex.Match(listingPrice.First().InnerText, @"\$([\d,.]+)");
                    string result = match.Success ? match.Groups[1].Value.Replace(",", "") : string.Empty;
                    if (result != string.Empty)
                    {
                        listedPrice = int.Parse(result);
                    }
                }
                // Get href to house page
                IList<HtmlNode> housePage = HTMLOperations.SelectCSS(listing.OuterHtml, "[class*=\"ember-view\"]");
                string path = housePage.First().GetAttributeValue("href", string.Empty);
                if (path != string.Empty) {
                    string html = ScrapeHousePage(path).GetAwaiter().GetResult();

                    IList<HtmlNode> keyFeatures = HTMLOperations.SelectCSS(html, "[data-test=\"features-icons\"]");
                    IList<HtmlNode> features = HTMLOperations.SelectCSS(keyFeatures.First().OuterHtml, ".items-center");
                    foreach (HtmlNode feature in features) {
                        string type = HTMLOperations.SelectCSS(feature.OuterHtml, "title").First().InnerText;
                        string value = HTMLOperations.SelectCSS(feature.OuterHtml, "span").First().InnerText.Trim();

                        if (type == "Bathroom") bathrooms = int.Parse(value);
                        if (type == "Bedroom") bedrooms = int.Parse(value);
                        if (type == "Land area") landArea = int.Parse(value.Remove(value.IndexOf('m')));
                        if (type == "Floor area") floorArea = int.Parse(value.Remove(value.IndexOf('m')));
                    }
                }
            }
        }
        
    }
}

using Geocoding.Google;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            realestate = JsonDocument.Parse(RealEstateVars.realestateString);
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
                return null;
            }
            string locale = address.Components[key].LongName;
            locale = locale.ToLower().Replace(" ", "-");
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
            string html = ScrapeWebsite(GetPath(address)).GetAwaiter().GetResult();
            _ = FindClosestHouse(GetStatisticalNodes(html), 3351, 180,4,2);
            return html;
        }

        public static RealEstateHouse FindClosestHouse(List<RealEstateHouse> list,int landArea, int floorArea,int bedrooms,int bathrooms)
        {
            RealEstateHouse currentClosest = null;
            float currentClosestVal = -1;

            foreach (RealEstateHouse house in list)
            {
                float closeness = 0;
                closeness += Math.Abs((float)((float)house.landArea - (float)landArea) / (float)(((float)house.landArea + (float)landArea) / 2f))*0.20f;
                closeness += Math.Abs((float)((float)house.floorArea - (float)floorArea) / (float)(((float)house.floorArea + (float)floorArea) / 2f))*0.40f;
                closeness += Math.Abs((float)((float)house.bedrooms - (float)bedrooms) / (float)(((float)house.bedrooms + (float)bedrooms) / 2f))*0.25f;
                closeness += Math.Abs((float)((float)house.bathrooms - (float)bathrooms) / (float)(((float)house.bathrooms + (float)bathrooms) / 2f)) * 0.15f;
                if (closeness < currentClosestVal || currentClosestVal == -1) { currentClosestVal = closeness;currentClosest = house; }
            }
            Console.WriteLine("Land Area: " + currentClosest.landArea + " Floor Area: " + currentClosest.floorArea + " Bedrooms: " + currentClosest.bedrooms + " Bathrooms: " + currentClosest.bathrooms+" Listed Price: "+currentClosest.listedPrice + "±" + (currentClosest.listedPrice * currentClosestVal) + " Closeness: "+ currentClosestVal);
            return currentClosest;
        }
        private static HttpClient httpClient = new HttpClient();
        static async Task<string> ScrapeWebsite(string path)
        {
            string htmlContent = "";
            int i = 1;
            while (true)
            {
                try
                {
                    string html = await httpClient.GetStringAsync("https://www.realestate.co.nz/residential/sale/" + path + "?page=" + i);
                    if(HTMLOperations.SelectCSS(html, ".tile--body").Count == 1) { break; }
                    htmlContent += html;
                    i++;
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
        public static List<RealEstateHouse> GetStatisticalNodes(string html)
        {
            List<List<string>> returnVal = new List<List<string>>();
            returnVal.Add(new List<string>());
            // Get Medians. (Sale Price, Asking Price, Rental Price)
            string selector = ".tile--body";
            IList<HtmlNode> listings = HTMLOperations.SelectCSS(html, selector);
            List<RealEstateHouse> houses = new List<RealEstateHouse>();
            int i = 1;
            foreach (HtmlNode node in listings)
            {
                Console.WriteLine(i + "/" +listings.Count);
                i++;
                houses.Add(new RealEstateHouse(node));
                
            }

            return houses;
        }
        public class RealEstateHouse
        {

            public int listedPrice;
            public int bedrooms;
            public int bathrooms;
            public int floorArea;
            public int landArea;
            private bool finished = false;

            public RealEstateHouse(HtmlNode listing)
            {
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
                if (path != string.Empty)
                {
                    ScrapeSpecificHouse(path);
                }
            }

            async void ScrapeSpecificHouse(string path)
            {
                string html = ScrapeHousePage(path).GetAwaiter().GetResult();

                IList<HtmlNode> keyFeatures = HTMLOperations.SelectCSS(html, "[data-test=\"features-icons\"]");
                IList<HtmlNode> features = HTMLOperations.SelectCSS(keyFeatures.First().OuterHtml, ".items-center");
                foreach (HtmlNode feature in features)
                {
                    string type = HTMLOperations.SelectCSS(feature.OuterHtml, "title").First().InnerText;
                    string value = HTMLOperations.SelectCSS(feature.OuterHtml, "span").First().InnerText.Trim();

                    if (type == "Bathroom") bathrooms = int.Parse(value);
                    if (type == "Bedroom") bedrooms = int.Parse(value);
                    if (type == "Land area") try { landArea = int.Parse(value.Remove(value.IndexOf('m'))); } catch { }
                    if (type == "Floor area") try { floorArea = int.Parse(value.Remove(value.IndexOf('m'))); } catch { }
                }
                finished = true;
            }
        }

    }
}

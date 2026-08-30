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
using System.Threading.Tasks;

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
            return ScrapeWebsite(GetPath(address)).GetAwaiter().GetResult();
        }
        private static HttpClient httpClient = new HttpClient();
        static async Task<string> ScrapeWebsite(string path)
        {
            try
            {
                string htmlContent = await httpClient.GetStringAsync("https://www.realestate.co.nz/insights/" + path);
                return htmlContent;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
                
            }
            
        }
        
    }
}

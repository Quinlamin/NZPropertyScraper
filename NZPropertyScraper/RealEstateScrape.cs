using Geocoding.Google;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace NZPropertyScraper
{
    public static class RealEstateScrape
    {
        public static JsonDocument realestate;
        public static void Scrape(GoogleAddress address)
        {
            realestate = JsonDocument.Parse(File.ReadAllText("JSON/realestate.json"));
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
                return;
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
                return;
            }
            path = path.Replace("_", "/");
            Console.WriteLine(path);
        }
    }
}

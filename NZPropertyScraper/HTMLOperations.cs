using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace NZPropertyScraper
{
    public static class HTMLOperations
    {
        public static IList<HtmlNode> SelectCSS(string html, string selector)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.QuerySelectorAll(selector);
        }
    }
}

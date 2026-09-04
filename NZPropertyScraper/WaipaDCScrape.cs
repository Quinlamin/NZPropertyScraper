using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
namespace NZPropertyScraper
{
    public static class WaipaDCScrape
    {
        public static string driverLocation = string.Empty;

        public static void Initialize(string path)
        {
            driverLocation = path;

        }

        public static void PropertyAndRatesWaipa()
        {
            using (var driver = new FirefoxDriver(driverLocation))
            {
                driver.Navigate().GoToUrl("https://waipadc.spatial.t1cloud.com/spatial/IntraMaps/ApplicationEngine/frontend/mapbuilder/default.htm?configId=6aa41407-1db8-44e1-8487-0b9a08965283&liteConfigId=9814f62a-448c-4a33-b101-4cf6cac0995a&title=UmF0ZXMlMjBJbmZvcm1hdGlvbg==");
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                IWebElement inputField;
                while (true)
                {
                    try
                    {
                        inputField = driver.FindElement(By.CssSelector("input"));
                        
                        break;
                    }
                    catch (Exception)
                    {

                        
                    }
                }
                Actions actions = new Actions(driver);
                inputField.Click();
                Console.ReadLine();
            }
        }
    }
}
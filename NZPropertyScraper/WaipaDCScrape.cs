using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace NZPropertyScraper
{
    public class Property
    {
        public int capVal;
        public int landVal;
        public float rates;
        public Dictionary<string, string> propertyValues;
        public Image satellite;
        public Property(Dictionary<string, string> _propertyValues)
        {
            foreach (var keyVal in _propertyValues)
            {
                if (keyVal.Key.Contains("Capital Value"))
                {
                    capVal = int.Parse(keyVal.Value.Replace("$", ""));
                }
                else if (keyVal.Key.Contains("Land Value"))
                {
                    landVal = int.Parse(keyVal.Value.Replace("$", ""));
                }
                else if (keyVal.Key.Contains("Total Rates"))
                {
                    rates = float.Parse(keyVal.Value.Replace("$", ""));
                }
            }
            propertyValues = _propertyValues;
        }
        public override string ToString()
        {
            return "Cap Val: " + capVal + " Land Val: " + landVal + " Rates: " + rates;
        }
    }
    public class WaipaDCScrape
    {
        public string driverLocation = string.Empty;
        
        public WaipaDCScrape(string _driverLocation)
        {
            driverLocation = _driverLocation;
        }
        
        public Property PropertyAndRatesWaipa(string inputValue)
        {

            Dictionary<string, string> propertyValues = new Dictionary<string, string>();
            FirefoxOptions options = new FirefoxOptions();
            
            options.AddArgument("--headless");
            
            using (var driver = new FirefoxDriver(options))
            {
                
                driver.Navigate().GoToUrl("https://waipadc.spatial.t1cloud.com/spatial/IntraMaps/ApplicationEngine/frontend/mapbuilder/default.htm?configId=6aa41407-1db8-44e1-8487-0b9a08965283&liteConfigId=9814f62a-448c-4a33-b101-4cf6cac0995a&title=UmF0ZXMlMjBJbmZvcm1hdGlvbg==");
                
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
                
                actions.MoveToElement(inputField).Click().Build().Perform();
                inputField.SendKeys(inputValue);

               
                var otherButton = driver.FindElement(By.CssSelector("button.mb-landscape-home"));
                actions.MoveToElement(otherButton).Click().Build().Perform();
                actions.MoveToElement(inputField).Click().Build().Perform();

                IWebElement result;
                while (true)
                {
                    try
                    {
                        result = driver.FindElement(By.CssSelector("li"));
                        break;
                    }
                    catch (Exception)
                    {

                        
                    }
                }
                result.Click();

                while (true)
                {
                    try
                    {
                        driver.FindElement(By.CssSelector("label.mb-fields-row__label"));
                        driver.FindElement(By.CssSelector(".mb-fields-row__value"));
                        break;
                    }
                    catch (Exception)
                    {


                    }
                }

                ReadOnlyCollection<IWebElement> fieldList = driver.FindElements(By.CssSelector("label.mb-fields-row__label"));
                ReadOnlyCollection<IWebElement> valueList = driver.FindElements(By.CssSelector(".mb-fields-row__value"));
                for (int i = 0; i < fieldList.Count; i++)
                {
                    //Console.WriteLine(fieldList[i].Text + " " + valueList[i].Text);
                    propertyValues.Add(fieldList[i].Text, valueList[i].Text);
                }

                
                string base64 = "";
                while(base64.Length< 1500000)
                {
                    var canvas = driver.FindElement(By.CssSelector("canvas"));
                    base64 = (string)driver.ExecuteScript("return arguments[0].toDataURL('image/png');", canvas);
                }
                Console.WriteLine(base64.Length);
                if (base64.Contains(","))
                {
                    base64 = base64.Split(',')[1];
                }
                
                byte[] imageBytes = Convert.FromBase64String(base64);
                
                File.WriteAllBytes("canvas_output.png", imageBytes);

            }
            Property property = new Property(propertyValues);
            property.satellite = Image.FromFile("canvas_output.png");
            
            return property;
        }
    }
}
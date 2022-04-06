using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Selenium
{
    public static class SeleniumDriverSelector
    {
        /*private static readonly Dictionary<string, WebDriver> webDrivers = new Dictionary<string, WebDriver>() { { "chromedriver.exe", new ChromeDriver() }, {"firefoxdriver.exe", new FirefoxDriver()} };

        public static WebDriver GetAvailableWebDriver()
        {
            foreach(KeyValuePair<string, WebDriver> entry in webDrivers)
            {
                if (File.Exists(entry.Key)) return entry.Value;
            }

            return null;
        }*/

        public static WebDriver GetAvailableWebDriver()
        {
            if (File.Exists("chromedriver.exe")) return new ChromeDriver();

            if (File.Exists("geckodriver.exe")) return new FirefoxDriver();

            return null;
        }
    }
}

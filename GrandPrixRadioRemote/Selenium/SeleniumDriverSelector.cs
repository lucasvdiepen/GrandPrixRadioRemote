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
        private static readonly Dictionary<string, Func<WebDriver>> webDrivers = new Dictionary<string, Func<WebDriver>>() 
        {
            { "chromedriver.exe", SeleniumChromeDriver.GetDriver },
            { "geckodriver.exe", SeleniumFirefoxDriver.GetDriver } 
        };

        public static WebDriver GetAvailableWebDriver()
        {
            foreach(KeyValuePair<string, Func<WebDriver>> entry in webDrivers)
            {
                if (File.Exists(entry.Key)) return entry.Value.Invoke();
            }

            return null;
        }
    }
}

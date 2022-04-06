using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Selenium
{
    public static class SeleniumFirefoxDriver
    {
        public static WebDriver GetDriver()
        {
            FirefoxOptions options = new FirefoxOptions();
            options.LogLevel = FirefoxDriverLogLevel.Fatal;

            return new FirefoxDriver(options);
        }
    }
}

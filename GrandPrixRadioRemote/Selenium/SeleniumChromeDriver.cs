using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GrandPrixRadioRemote.Utils;

namespace GrandPrixRadioRemote.Selenium
{
    public static class SeleniumChromeDriver
    {
        public static WebDriver GetDriver()
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--silent");
            options.AddArgument("log-level=3");
            options.AddArgument("--output=/dev/null");

            ChromeDriver chromeDriver = null;

            try
            {
                chromeDriver = new ChromeDriver(service, options);
            }
            catch(InvalidOperationException)
            {
                ErrorHandlerUtility.ShowError("This chromedriver version does not match the browser version");
            }

            return chromeDriver;
        }
    }
}

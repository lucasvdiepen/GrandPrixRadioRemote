using GrandPrixRadioRemote.Enums;
using GrandPrixRadioRemote.DataClasses;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Selenium
{
    public class SeleniumDriver
    {
        private WebDriver driver;

        public bool Initialized { get; private set; } = true;

        public SeleniumDriver(string url)
        {
            Setup(url);
        }

        private void Setup(string url)
        {
            driver = SeleniumDriverSelector.GetAvailableWebDriver();
            if(driver == null)
            {
                Initialized = false;
                return;
            }

            driver.Navigate().GoToUrl(url);
        }

        public void Stop()
        {
            driver.Quit();
        }

        public By GetBy(GrandPrixRadioRemote.DataClasses.WebElement element)
        {
            switch (element.Type)
            {
                case FindElementType.Id:
                    return By.Id(element.Name);
                case FindElementType.Class:
                    return By.ClassName(element.Name);
            }

            return null;
        }

        public void ClickButton(IWebElement webElement)
        {
            if (webElement == null) return;

            try
            {
                webElement.Click();
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        public IWebElement GetWebElement(By by)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (Exception) { }

            return null;
        }

        public ReadOnlyCollection<IWebElement> GetWebElements(By by)
        {
            try
            {
                return driver.FindElements(by);
            }
            catch (Exception) { }

            return null;
        }

        public void ExecuteScript(string script)
        {
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript(script);
        }

        public void Reload()
        {
            driver.Navigate().Refresh();
        }

        public void ClickAndHoldButton(IWebElement webElement, int timeInMiliseconds)
        {
            Actions actions = new Actions(driver);
            actions.ClickAndHold(webElement).Perform();

            Thread.Sleep(timeInMiliseconds);

            actions.Release(webElement).Perform();
        }
    }
}

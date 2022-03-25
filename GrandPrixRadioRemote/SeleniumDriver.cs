using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote
{
    public class SeleniumDriver
    {
        private ChromeDriver driver;

        public SeleniumDriver(string url)
        {
            Setup(url);
        }

        private void Setup(string url)
        {
            driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);
        }

        public void Stop()
        {
            driver.Quit();
        }

        public By GetBy(WebElement element)
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
            try
            {
                webElement.Click();
            }
            catch (Exception) { }
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
    }
}

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
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

        public void ClickButton(WebElement element)
        {
            ClickButton(GetBy(element));
        }

        private By GetBy(WebElement element)
        {
            switch(element.Type)
            {
                case FindElementType.Id:
                    return By.Id(element.Name);
                case FindElementType.Class:
                    return By.ClassName(element.Name);
            }

            return null;
        }

        private void ClickButton(By by)
        {
            try
            {
                IWebElement button = driver.FindElement(by);
                button.Click();
            }
            catch (Exception) { }
        }
    }
}

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

        public void ClickButtonById(string id)
        {
            ClickButton(By.Id(id));
        }

        public void ClickButtonByClass(string className)
        {
            ClickButton(By.ClassName(className));
        }

        private void ClickButton(By by)
        {
            IWebElement button = driver.FindElement(by);
            button.Click();
        }
    }
}

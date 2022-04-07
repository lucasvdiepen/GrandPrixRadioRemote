using GrandPrixRadioRemote.DataClasses;
using GrandPrixRadioRemote.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GrandPrixRadioRemote.Utils
{
    public static class XMLReaderUtility
    {
        public static WebElement GetWebElement(string name)
        {
            if(GetXDocument(EmbeddedFileReaderUtility.ReadFile(FilePath.WebElements), out XDocument xmlDoc))
            {
                XElement webElementRoot = xmlDoc.Root.Element(name);

                return new WebElement((FindElementType)Enum.Parse(typeof(FindElementType), webElementRoot.Element("Type").Value), webElementRoot.Element("Name").Value);
            }

            return null;
        }

        public static Config GetConfig()
        {
            if(GetXDocument(GetXMLFile(FilePath.Config), out XDocument xmlDoc))
            {
                XElement port = xmlDoc.Root.Element("Port");

                Config config = null;

                if(int.TryParse(port.Value, out int result))
                {
                    config = new Config(result);
                }

                return config;
            }

            return null;
        }

        public static string GetElement(string xml, string elementName)
        {
            if(GetXDocument(xml, out XDocument xmlDoc))
            {
                return xmlDoc.Root.Element(elementName).Value;
            }

            return null;
        }

        private static bool GetXDocument(string xml, out XDocument xDocument)
        {
            xDocument = null;

            try
            {
                xDocument = XDocument.Parse(xml);
                return true;
            }
            catch (System.Xml.XmlException) { }

            return false;
        }

        private static string GetXMLFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}

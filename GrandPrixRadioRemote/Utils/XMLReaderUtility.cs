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
            XDocument xmlDoc = GetEmbeddedXMLDocument(FilePath.WebElements);
            XElement webElementRoot = xmlDoc.Root.Element(name);

            return new WebElement((FindElementType)Enum.Parse(typeof(FindElementType), webElementRoot.Element("Type").Value), webElementRoot.Element("Name").Value);
        }

        public static Config GetConfig()
        {
            XDocument xmlDoc = GetXMLDocument(FilePath.Config);
            XElement port = xmlDoc.Root.Element("Port");

            Config config = new Config(int.Parse(port.Value));

            return config;
        }

        private static XDocument GetXMLDocument(string path)
        {
            return XDocument.Parse(File.ReadAllText(path));
        }

        private static XDocument GetEmbeddedXMLDocument(string path)
        {
            return XDocument.Parse(EmbeddedFileReaderUtility.ReadFile(path));
        }
    }
}

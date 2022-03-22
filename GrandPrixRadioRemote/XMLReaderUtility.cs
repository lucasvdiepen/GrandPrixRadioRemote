﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GrandPrixRadioRemote
{
    public static class XMLReaderUtility
    {
        public static WebElement GetWebElement(string name)
        {
            XDocument xmlDoc = GetXMLDocument();
            XElement webElementRoot = xmlDoc.Root.Element(name);

            return new WebElement((FindElementType)Enum.Parse(typeof(FindElementType), webElementRoot.Element("Type").Value), webElementRoot.Element("Name").Value);
        }

        private static XDocument GetXMLDocument()
        {
            return XDocument.Parse(File.ReadAllText("WebElements.xml"));
        }
    }
}

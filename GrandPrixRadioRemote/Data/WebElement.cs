using GrandPrixRadioRemote.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Data
{
    public class WebElement
    {
        public FindElementType Type { get; private set; }
        public string Name { get; private set; }

        public WebElement(FindElementType type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}

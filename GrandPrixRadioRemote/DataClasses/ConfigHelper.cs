using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrandPrixRadioRemote.Enums;
using GrandPrixRadioRemote.Utils;

namespace GrandPrixRadioRemote.DataClasses
{
    public static class ConfigHelper
    {
        private const string defaultConfig =
            "<Config>\n" +
            "  <Port>9191</Port>\n" +
            "</Config>";

        public static Config GetConfig()
        {
            Config config = XMLReaderUtility.GetConfig();
            if(config == null)
            {
                CreateDefaultConfig();
                config = new Config(int.Parse(XMLReaderUtility.GetElement(defaultConfig, "Port")));
            }

            return config;
        }

        private static void CreateDefaultConfig()
        {
            File.WriteAllText(FilePath.Config, defaultConfig);

            Console.WriteLine("Created new config.xml");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Utils
{
    public static class EmbeddedFileReaderUtility
    {
        public static string ReadFile(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "GrandPrixRadioRemote." + path;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

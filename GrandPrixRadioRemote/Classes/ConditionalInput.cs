using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GrandPrixRadioRemote.Classes
{
    public static class ConditionalInput
    {
        public static bool GetInput(string text, string[] positiveInput, string[] negativeInput)
        {
            while (true)
            {
                Console.Write(text);
                string input = Console.ReadLine();
                if (positiveInput.Contains(input)) return true;

                if (negativeInput.Contains(input)) return false;

                Console.WriteLine("Invalid input. Please try again.");
            }
        }
    }
}

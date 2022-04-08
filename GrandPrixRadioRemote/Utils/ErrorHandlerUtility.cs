using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Utils
{
    public static class ErrorHandlerUtility
    {
        public static void ShowError(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("\nPress enter to exit...");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiuiEULocalization.Utilities
{
    public class ConsoleWrapper
    {
        public static void WriteInfo(string module, bool? result, string message)
        {
            if (result == null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{module}] - {message}");
            }
            else if (result == true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{module}] O {message}");
            }
            else if (result == false)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{module}] X {message}");
            }
            Console.ResetColor();
        }

        public static void WriteError(string module, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{module}] ! {message}");
            Console.ResetColor();
        }
    }
}

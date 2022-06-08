using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppeteerHandler
{
    internal static class Log
    {
        public static void NL() => Console.WriteLine("");

        public static void W(object Message, ConsoleColor Color = ConsoleColor.Gray)
        {
            string Out = Message?.ToString() ?? "NULL";

            Console.ForegroundColor = Color;
            Console.Write(Out);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void WL(object Message, ConsoleColor Color = ConsoleColor.Gray)
        {
            string Out = Message?.ToString() ?? "NULL";

            Console.ForegroundColor = Color;
            Console.WriteLine(Out);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}

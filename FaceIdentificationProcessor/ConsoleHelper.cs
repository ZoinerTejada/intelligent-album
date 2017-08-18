using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceIdentificationProcessor
{
    public static class ConsoleHelper
    {
        public static void WriteLineInColor(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void WriteInColor(string msg, ConsoleColor color, int? x = null, int? y = null)
        {
            if (x.HasValue && y.HasValue)
            {
                Console.SetCursorPosition(x.Value, y.Value);
            }
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }
    }
}

using MonoBrickFirmware.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoTest2
{
    public static class LegoUtils
    {
        private const int CHARS_PER_LINE = 24;
        public static void LongStringPrint(string content)
        {
            int linesPrinted = 0;

            while ((linesPrinted * CHARS_PER_LINE) < content.Length)
            {
                LcdConsole.WriteLine(content.Substring(linesPrinted * CHARS_PER_LINE, CHARS_PER_LINE));
                linesPrinted++;
            }
        }

        public static void NullPrint(string name, object o)
        {
            if (o == null)
            {
                LcdConsole.WriteLine("{0} is null", name);
            }
        }

        public static double Max3(double a, double b, double c)
        {
            return Math.Max(Math.Max(a, b), c);
        }
    }
}

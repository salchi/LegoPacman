using MonoBrickFirmware.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LegoPacman.classes
{
    public static class LegoUtils
    {
        private const int CHARS_PER_LINE = 24;
        public static void PrintLongString(string content)
        {
            while (content.Length > CHARS_PER_LINE)
            {
                LcdConsole.WriteLine(content.Substring(0, CHARS_PER_LINE));
                content = content.Substring(CHARS_PER_LINE - 1, Math.Min(CHARS_PER_LINE, content.Length - CHARS_PER_LINE));
            }
            LcdConsole.WriteLine(content);
        }

        public static void PrintAndWait(int durationInSeconds, string s, params object[] objects)
        {
            LcdConsole.WriteLine(s, objects);
            Thread.Sleep(durationInSeconds * 1000);
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

        const double DIAMETER = 3.4;
        public static uint CmToEngineDegrees(double distanceInCm)
        {
            return (uint)Math.Round((360 * distanceInCm) / (Math.PI * DIAMETER / 2));
        }
    }
}

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
        public static void WaitOnHandle(WaitHandle handle)
        {
            handle.WaitOne();
        }

        public static Predicate<T> Negate<T>(Predicate<T> predicate)
        {
            return x => !predicate(x);
        }

        public static void PrintLongString(string content)
        {
            const int CharsPerLine = 28;
            for (int charsPrinted = 0; charsPrinted < content.Length;)
            {
                int charsToPrint = Math.Min(content.Length - charsPrinted, CharsPerLine);
                LcdConsole.WriteLine(content.Substring(charsPrinted, charsToPrint));
                charsPrinted += charsToPrint;
            }
        }

        public static void PrintAndWait(int durationInSeconds, string line, params object[] objects)
        {
            LcdConsole.WriteLine(line, objects);
            Thread.Sleep(durationInSeconds * 1000);
        }

        public static void NullPrint(string objectName, object o)
        {
            if (o == null)
            {
                LcdConsole.WriteLine("{0} is null", objectName);
            }
        }
    }
}

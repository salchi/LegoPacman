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

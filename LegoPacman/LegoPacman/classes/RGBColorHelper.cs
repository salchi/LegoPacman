using MonoBrickFirmware.Display;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LegoTest2
{
    public static class RGBColorHelper
    {
        public static String ToString(RGBColor a)
        {
            return String.Format("({0},{1},{2}", a.Red, a.Green, a.Blue);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoPacman.classes
{
    public static class LegoMath
    {
        public static int DoubleToInt(double number)
        {
            return (int)Math.Round(number);
        }

        public static int AbsDelta(int number1, int number2)
        {
            return Math.Abs(number1 - number2);
        }

        public static double Max3(double a, double b, double c)
        {
            return Math.Max(Math.Max(a, b), c);
        }

        const double WheelDiameter = 3.4;
        public static uint CmToEngineDegrees(double distanceInCm)
        {
            return (uint)Math.Round((360 * distanceInCm) / (Math.PI * WheelDiameter / 2));
        }
    }
}

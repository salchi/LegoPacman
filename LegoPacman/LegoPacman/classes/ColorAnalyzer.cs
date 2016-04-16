using MonoBrickFirmware.Display;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoTest2
{
    public class ColorAnalyzer
    {
        public ConcurrentBag<KnownColor> ValidColors { get; }

        private const double MAX_ALLOWED_SPIKE_DISTANCE = 35d;
        private const double MAX_AVG_DISTANCE = 25d;

        public ColorAnalyzer()
        {
           ValidColors = new ConcurrentBag<KnownColor>();
        }
        
        public KnownColor Analyze(RGBColor c)
        {
            double currentDistance= MAX_AVG_DISTANCE;
            KnownColor result = KnownColor.Invalid;

            foreach(KnownColor kc in ValidColors)
            {
                double tempDistance = AverageDistance(kc, c);
                LcdConsole.WriteLine("{0} {1} {2}", kc, tempDistance, MaxDistance(kc, c));

                if (SpikeTest(kc, c) && tempDistance < currentDistance)
                {
                    currentDistance = tempDistance;
                    result = kc;
                }
            }

            return result;
        }

        private static bool SpikeTest(KnownColor kc, RGBColor c)
        {
            return MaxDistance(kc, c) <= MAX_ALLOWED_SPIKE_DISTANCE;
        }

        private static double MaxDistance(KnownColor kc, RGBColor c)
        {
            RGBColor kcRgb = kc.TargetColor;

            return LegoUtils.Max3(Math.Abs(kcRgb.Red - c.Red), Math.Abs(kcRgb.Green - c.Green), Math.Abs(kcRgb.Blue - c.Blue));
        }

        private static double AverageDistance(KnownColor kc, RGBColor c)
        {
            RGBColor kcRgb = kc.TargetColor;

            return (Math.Abs(kcRgb.Red - c.Red) + Math.Abs(kcRgb.Green - c.Green) + Math.Abs(kcRgb.Blue - c.Blue)) / 3d;
        }
    }
}

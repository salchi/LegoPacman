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
        public ConcurrentBag<KnownColor> ValidColors { get; } = new ConcurrentBag<KnownColor>();

        private static readonly double maxAllowedSpikeDistance = 35d;
        private static readonly double maxAvgDistance = 25d;
        public KnownColor Analyze(RGBColor c)
        {
            double currentDistance= maxAvgDistance;
            KnownColor result = KnownColor.Invalid;

            foreach(KnownColor kc in ValidColors)
            {
                double tempDistance = averageDistance(kc, c);
                LcdConsole.WriteLine("{0} {1} {2}", kc, tempDistance, maxDistance(kc, c));

                if (spikeTest(kc, c) && tempDistance < currentDistance)
                {
                    currentDistance = tempDistance;
                    result = kc;
                }
            }

            return result;
        }

        private static bool spikeTest(KnownColor kc, RGBColor c)
        {
            return maxDistance(kc, c) <= maxAllowedSpikeDistance;
        }

        private static double maxDistance(KnownColor kc, RGBColor c)
        {
            RGBColor kcRgb = kc.TargetColor;

            return LegoUtils.Max3(Math.Abs(kcRgb.Red - c.Red), Math.Abs(kcRgb.Green - c.Green), Math.Abs(kcRgb.Blue - c.Blue));
        }

        private static double averageDistance(KnownColor kc, RGBColor c)
        {
            RGBColor kcRgb = kc.TargetColor;

            return (Math.Abs(kcRgb.Red - c.Red) + Math.Abs(kcRgb.Green - c.Green) + Math.Abs(kcRgb.Blue - c.Blue)) / 3d;
        }
    }
}

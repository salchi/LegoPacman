using MonoBrickFirmware.Display;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoPacman.classes
{
    public class ColorAnalyzer
    {
        public List<KnownColor> ValidColors { get; }

        private const double MAX_ALLOWED_SPIKE_DISTANCE = 35d;
        private const double MAX_AVG_DISTANCE = 25d;

        public ColorAnalyzer()
        {
           ValidColors = new List<KnownColor>();
        }
        
        public KnownColor Analyze(RGBColor c)
        {
            var currentDistance = MAX_AVG_DISTANCE;
            var result = KnownColor.Invalid;

            foreach(var kc in ValidColors)
            {
                var tempDistance = AverageDistance(kc, c);
                //LcdConsole.WriteLine("{0} {1} {2}", kc, tempDistance, MaxDistance(kc, c));

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
            var kcRgb = kc.TargetColor;

            return LegoUtils.Max3(Math.Abs(kcRgb.Red - c.Red), Math.Abs(kcRgb.Green - c.Green), Math.Abs(kcRgb.Blue - c.Blue));
        }

        private static double AverageDistance(KnownColor kc, RGBColor c)
        {
            var kcRgb = kc.TargetColor;

            return (Math.Abs(kcRgb.Red - c.Red) + Math.Abs(kcRgb.Green - c.Green) + Math.Abs(kcRgb.Blue - c.Blue)) / 3d;
        }
    }
}

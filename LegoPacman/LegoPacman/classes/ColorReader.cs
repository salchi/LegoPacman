using MonoBrickFirmware.Display;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoTest2
{
    public class ColorReader
    {
        private readonly EV3ColorSensor colorSensor;
        public RGBColor LastRead { get; private set; }

        public ColorReader(SensorPort colorPort)
        {
            colorSensor = new EV3ColorSensor(colorPort, ColorMode.RGB);
            LastRead = KnownColor.Invalid.TargetColor;
        }

        public bool TryRead()
        {
            colorSensor.Mode = ColorMode.RGB;
            LastRead = colorSensor.ReadRGB();
            return IsValidColor(LastRead);
        }

        private static readonly double lowInvalidThreshold = 4d;
        private static readonly double spikeValidThreshold = 8d;
        private bool IsValidColor(RGBColor c)
        {
            return c.Red > lowInvalidThreshold && c.Green > lowInvalidThreshold
                   && c.Blue > lowInvalidThreshold || LegoUtils.Max3(c.Red, c.Green, c.Blue) >= spikeValidThreshold;
        }
    }
}

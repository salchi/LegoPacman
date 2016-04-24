using MonoBrickFirmware.Display;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoPacman.classes
{
    public class ColorReader
    {
        private readonly EV3ColorSensor colorSensor;
        public RGBColor LastRead { get; private set; }

        public ColorReader(SensorPort colorPort)
        {
            colorSensor = new EV3ColorSensor(colorPort, ColorMode.RGB);
            LastRead = KnownColor.Invalid.RgbDefinition;
        }

        public bool TryRead()
        {
            colorSensor.Mode = ColorMode.RGB;
            LastRead = colorSensor.ReadRGB();
            return IsValidColor(LastRead);
        }

        private bool IsValidColor(RGBColor c)
        {
            const double LowInvalidThreshold = 4d;
            const double SpikeValidThreshold = 8d;

            return (c.Red > LowInvalidThreshold && c.Green > LowInvalidThreshold &&
                c.Blue > LowInvalidThreshold) || LegoMath.Max3(c.Red, c.Green, c.Blue) >= SpikeValidThreshold;
        }
    }
}

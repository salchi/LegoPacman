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
        private const double LOW_INVALID_THRESHOLD = 4d;
        private const double SPIKE_VALID_THRESHOLD = 8d;

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

        private bool IsValidColor(RGBColor c)
        {
            return c.Red > LOW_INVALID_THRESHOLD && c.Green > LOW_INVALID_THRESHOLD && 
                c.Blue > LOW_INVALID_THRESHOLD || LegoUtils.Max3(c.Red, c.Green, c.Blue) >= SPIKE_VALID_THRESHOLD;
        }
    }
}

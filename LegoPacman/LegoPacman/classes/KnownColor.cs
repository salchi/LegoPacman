using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoTest2
{
    public class KnownColor
    {
        public static readonly KnownColor Invalid = new KnownColor("Invalid", new RGBColor(0, 0, 0));
        public static readonly KnownColor Red = new KnownColor("Red", new RGBColor(255, 0, 0));
        public static readonly KnownColor Green = new KnownColor("Green", new RGBColor(0, 255, 0));
        public static readonly KnownColor Blue = new KnownColor("Blue", new RGBColor(0, 0, 255));
        public static readonly List<KnownColor> AllValid = new List<KnownColor> { Red, Green, Blue };

        public string Name { get; }
        public RGBColor TargetColor { get; }

        public KnownColor(string name, RGBColor targetColor)
        {
            Name = name;
            TargetColor = targetColor;
        }

        public override string ToString()
        {
            return String.Format("KC({0}, {1})", Name, RGBColorHelper.ToString(TargetColor));
        }
    }
}

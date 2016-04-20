using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoPacman.classes
{
    public class KnownColor
    {
        public static readonly KnownColor Invalid = new KnownColor("Invalid", new RGBColor(0, 0, 0));
        public static readonly KnownColor Fence_temp = new KnownColor("Fence", new RGBColor(20, 30, 15));
        public static readonly KnownColor Blue = new KnownColor("Fence", new RGBColor(26, 83, 93));

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

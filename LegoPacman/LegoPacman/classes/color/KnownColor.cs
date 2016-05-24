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
        public static readonly KnownColor Invalid = new KnownColor("Invalid", new RGBColor(8, 8, 8));
        /*
        Alphatest
        ----------------------
        public static readonly KnownColor Fence_temp = new KnownColor("Fence", new RGBColor(203, 168, 80));
        public static readonly KnownColor Blue = new KnownColor("Blue", new RGBColor(20, 75, 93));
        public static readonly KnownColor Red = new KnownColor("Red", new RGBColor(175, 21, 10));
        public static readonly KnownColor Yellow = new KnownColor("Yellow", new RGBColor(237, 187, 34));
        public static readonly KnownColor White = new KnownColor("White", new RGBColor(280, 291, 177));*/

        public static readonly KnownColor FenceLight = new KnownColor("FenceLight", new RGBColor(263, 162, 89));
        public static readonly KnownColor FenceDark = new KnownColor("FenceDark", new RGBColor(230, 140, 64));
        public static readonly KnownColor Blue = new KnownColor("Blue", new RGBColor(51, 86, 103));
        public static readonly KnownColor Red = new KnownColor("Red", new RGBColor(290, 35, 16));
        public static readonly KnownColor Yellow = new KnownColor("Yellow", new RGBColor(322, 220, 43));
        public static readonly KnownColor White = new KnownColor("White", new RGBColor(369, 332, 210));

        /*
        home
        -------------------
        public static readonly KnownColor Fence_temp = new KnownColor("Fence", new RGBColor(30, 34, 19));
        public static readonly KnownColor Blue = new KnownColor("Blue", new RGBColor(25, 37, 35));
        public static readonly KnownColor Red = new KnownColor("Red", new RGBColor(148, 30, 16));
        public static readonly KnownColor Yellow = new KnownColor("Yellow", new RGBColor(289, 136, 35));
        public static readonly KnownColor White = new KnownColor("White", new RGBColor(326, 281, 91));*/

        public static List<KnownColor> ActionColors = new List<KnownColor>() { Blue, Red, Yellow, White };
        public static List<KnownColor> FenceColors = new List<KnownColor>() { FenceDark, FenceLight };

        public string Name { get; }
        public RGBColor RgbDefinition { get; }

        public KnownColor(string name, RGBColor definition)
        {
            Name = name;
            RgbDefinition = definition;
        }

        public static bool IsActionColor(KnownColor color)
        {
            return ActionColors.Contains(color);
        }

        public static bool IsFenceColor(KnownColor color)
        {
            return FenceColors.Contains(color);
        }

        public override string ToString()
        {
            return String.Format("KC({0}, {1})", Name, RGBColorHelper.ToString(RgbDefinition));
        }
    }
}

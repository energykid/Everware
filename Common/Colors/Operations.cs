using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everware.Common.Colors;

public static partial class ColorExtensions
{
    extension(Color)
    {
        public static Color Min(Color colorA, Color colorB)
        {
            colorA.R = Math.Min(colorA.R, colorB.R);
            colorA.G = Math.Min(colorA.G, colorB.G);
            colorA.B = Math.Min(colorA.B, colorB.B);
            colorA.A = Math.Min(colorA.A, colorB.A);

            return colorA;
        }

        public static Color Max(Color colorA, Color colorB)
        {
            colorA.R = Math.Max(colorA.R, colorB.R);
            colorA.G = Math.Max(colorA.G, colorB.G);
            colorA.B = Math.Max(colorA.B, colorB.B);
            colorA.A = Math.Max(colorA.A, colorB.A);

            return colorA;
        }
    }
}

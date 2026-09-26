using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Everware.Common.Colors;

public static partial class ColorExtensions
{
    extension(ref Color color)
    {
        public float Hue
        {
            get
            {
                var highest = Math.Max(color.R, color.G);
                highest = Math.Max(highest, color.B);

                var lowest = Math.Min(color.R, color.G);
                lowest = Math.Min(lowest, color.B);

                var range = highest - lowest;

                var r2 = (highest - color.R) / range;
                var g2 = (highest - color.G) / range;
                var b2 = (highest - color.B) / range;

                if (color.R == highest)
                {
                    return (color.G == lowest ? 5f + b2 : 1f - g2) / 6f;
                }

                if (color.G == highest)
                {
                    return (color.B == lowest ? 1f + r2 : 3f - b2) / 6f;
                }

                return (color.R == lowest ? 3f + g2 : 5f - r2) / 6f;
            }

            set
            {
                var newColor = Color.FromHsl(value, color.Saturation, color.Lightness);

                color.R = newColor.R;
                color.G = newColor.G;
                color.B = newColor.B;
            }
        }

        public float Saturation
        {
            get
            {
                var highest = Math.Max(color.R, color.G);
                highest = Math.Max(highest, color.B);

                var lowest = Math.Min(color.R, color.G);
                lowest = Math.Min(lowest, color.B);

                var range = highest - lowest;

                var lightness = (lowest + highest) * 0.5f;

                return range / (lightness <= 0.5f ? (highest + lowest) : (2f - highest - lowest));
            }

            set
            {
                var newColor = Color.FromHsl(color.Hue, value, color.Lightness);

                color.R = newColor.R;
                color.G = newColor.G;
                color.B = newColor.B;
            }
        }

        public float Lightness
        {
            get
            {
                var highest = Math.Max(color.R, color.G);
                highest = Math.Max(highest, color.B);

                var lowest = Math.Min(color.R, color.G);
                lowest = Math.Min(lowest, color.B);

                return (lowest + highest) / 2f;
            }

            set
            {
                var newColor = Color.FromHsl(color.Hue, color.Saturation, value);

                color.R = newColor.R;
                color.G = newColor.G;
                color.B = newColor.B;
            }
        }
    }

    extension(Color color)
    {
        public static Color FromHsl(float hue, float saturation, float lightness)
        {
            return Main.hslToRgb(hue, saturation, lightness);
        }

        public static Color FromHsl(Vector3 hsl)
        {
            return Main.hslToRgb(hsl.X, hsl.Y, hsl.Z);
        }

        public static Color HslLerp(Color colorA, Color colorB, float factor, bool useLongHueInterpolation = false)
        {
            var hslA = colorA.ToHsl();
            var hslB = colorB.ToHsl();

            if (MathF.Abs(hslB.X - hslA.X) > 0.5f != useLongHueInterpolation)
            {
                return Color.FromHsl(WrapLerp(hslA.X, hslB.X), Lerp(hslA.X, hslB.X), Lerp(hslA.X, hslB.X));
            }
            else
            {
                return Color.FromHsl(Lerp(hslA.X, hslB.X), Lerp(hslA.X, hslB.X), Lerp(hslA.X, hslB.X));
            }

            float Lerp(float a, float b)
            {
                return MathHelper.Lerp(a, b, factor);
            }

            float WrapLerp(float a, float b)
            {
                if (a < b)
                {
                    a += 1;

                    return Lerp(a, b) % 1f;
                }

                b += 1;

                return Lerp(a, b) % 1f;
            }
        }

        public Vector3 ToHsl()
        {
            var highest = Math.Max(Math.Max(color.R, color.G), color.B);
            var lowest = Math.Min(Math.Min(color.R, color.G), color.B);

            var range = highest - lowest;

            var r2 = (highest - color.R) / range;
            var g2 = (highest - color.G) / range;
            var b2 = (highest - color.B) / range;

            var hue = GetHue();

            var lightness = (lowest + highest) * 0.5f;

            var saturation = range / (lightness <= 0.5f ? (highest + lowest) : (2f - highest - lowest));

            return new Vector3(hue, saturation, lightness);

            float GetHue()
            {
                if (color.R == highest)
                {
                    return (color.G == lowest ? 5f + b2 : 1f - g2) / 6f;
                }

                if (color.G == highest)
                {
                    return (color.B == lowest ? 1f + r2 : 3f - b2) / 6f;
                }

                return (color.R == lowest ? 3f + g2 : 5f - r2) / 6f;
            }
        }
    }
}

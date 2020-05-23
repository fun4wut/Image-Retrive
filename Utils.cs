using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace Utils
{
    public static class PixelUtils
    {
        public static (double H, double S, double V) RGB2HSV(Rgb24 pixel)
        {   
            var (r, g, b) = (pixel.R / 255.0, pixel.G / 255.0, pixel.B / 255.0);
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var delta = max - min;
            double h = 0, s, v;
            
            if (max == min) h = 0;
            else if (max == r) h = (60 * ((g - b) / delta) + 360) % 360;
            else if (max == g) h = (60 * ((b - r) / delta) + 120) % 360;
            else if (max == b) h = (60 * ((r - g) / delta) + 240) % 360;
            
            s = max == 0 ? 0 : delta / max;
            v = max;
            // Console.WriteLine((max, min, h, s, v));           
            return (h, s, v);
        }

        public static int Quantilize((double, double, double) pixel)
        {
            var (h, s, v) = pixel;
            var hlist = new int[]{ 20, 40, 75, 155, 190, 270, 290, 316, 360};
            var slist = new double[]{ 0.25, 0.7, 1.0 };
            var vlist = new double[]{ 0.3, 0.8, 1.0 };
            for (int i = 0; i < hlist.Length; i++) if (h <= hlist[i]) { h = i % 8; break; }
            for (int i = 0; i < slist.Length; i++) if (s <= slist[i]) { s = i; break; }
            for (int i = 0; i < vlist.Length; i++) if (v <= vlist[i]) { v = i; break; }
            return (int)(9 * h + 3 * s + v);
        }
    
    }

}
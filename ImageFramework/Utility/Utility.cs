﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageFramework.Utility
{
    public static class Utility
    {
        /// <summary>
        /// transforms coordinates from [-1, 1] to [0, imagesize - 1].
        /// clamps values if coordinates are not within range
        /// </summary>
        /// <param name="coord">[-1, 1]</param>
        /// <param name="imageWidth">width in pixels</param>
        /// <param name="imageHeight">height in pixel</param>
        /// <returns></returns>
        /*public static Point CanonicalToTexelCoordinates(Vector2 coord, int imageWidth, int imageHeight)
        {
            // trans mouse is betweem [-1,1] in texture coordinates => to [0,1]
            coord.X += 1.0f;
            coord.X /= 2.0f;

            coord.Y += 1.0f;
            coord.Y /= 2.0f;

            // clamp value
            coord.X = Math.Min(0.9999f, Math.Max(0.0f, coord.X));
            coord.Y = Math.Min(0.9999f, Math.Max(0.0f, coord.Y));

            // scale with mipmap level
            coord.X *= (float)imageWidth;
            coord.Y *= (float)imageHeight;

            return new Point((int)(coord.X), (int)(coord.Y));
        }*/

        /// <summary>
        /// opens the file dialog for images
        /// </summary>
        /// <returns>string with filenames or null if aborted</returns>
        /*public static string[] ShowImportImageDialog(Window parent)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = Properties.Settings.Default.ImagePath
            };

            if (ofd.ShowDialog(parent) != true) return null;

            // set new image path in settings
            Properties.Settings.Default.ImagePath = System.IO.Path.GetDirectoryName(ofd.FileName);
            return ofd.FileNames;
        }*/

        /// <summary>
        /// calculates a/b and adds one if the remainder (a%b) is not zero
        /// </summary>
        /// <param name="a">nominator</param>
        /// <param name="b">denominator</param>
        /// <returns></returns>
        public static int DivideRoundUp(int a, int b)
        {
            Debug.Assert(b > 0);
            Debug.Assert(a >= 0);
            return (a + b - 1) / b;
        }

        public static int AlignTo(int size, int alignment)
        {
            if (size % alignment == 0) return size;
            return size + alignment - (size % alignment);
        }

        public static string FromSrgbFunction()
        {
            return
                @"
float4 fromSrgb(float4 c){
    float3 r;
    [unroll]
    for(int i = 0; i < 3; ++i){
        if(c[i] > 1.0) r[i] = 1.0;
        else if(c[i] < 0.0) r[i] = 0.0;
        else if(c[i] <= 0.04045) r[i] = c[i] / 12.92;
        else r[i] = pow((c[i] + 0.055)/1.055, 2.4);
    }
    return float4(r, c.a);
}";
        }

        public static string ToSrgbFunction()
        {
            return
                @"
float4 toSrgb(float4 c){
    float3 r;
    [unroll]
    for(int i = 0; i < 3; ++i){
        if( c[i] > 1.0) r[i] = 1.0;
        else if( c[i] < 0.0) r[i] = 0.0;
        else if( c[i] <= 0.0031308) r[i] = 12.92 * c[i];
        else r[i] = 1.055 * pow(c[i], 0.41666) - 0.055;
    }
    return float4(r, c.a);
}";
        }

        public struct Int2
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// transforms coordinates from [-1, 1] to [0, imagesize - 1].
        /// clamps values if coordinates are not within range
        /// </summary>
        /// <param name="x">[-1, 1]</param>
        /// <param name="y">[-1, 1]</param>
        /// <param name="imageWidth">width in pixels</param>
        /// <param name="imageHeight">height in pixel</param>
        /// <returns></returns>
        public static Int2 CanonicalToTexelCoordinates(float x, float y, int imageWidth, int imageHeight)
        {
            // trans mouse is betweem [-1,1] in texture coordinates => to [0,1]
            x += 1.0f;
            x /= 2.0f;

            y += 1.0f;
            y /= 2.0f;

            // clamp value
            x = Math.Min(0.9999f, Math.Max(0.0f, x));
            y = Math.Min(0.9999f, Math.Max(0.0f, y));

            // scale with mipmap level
            x *= (float)imageWidth;
            y *= (float)imageHeight;

            return new Int2{X = (int)(x), Y = (int)(y)};
        }
    }
}

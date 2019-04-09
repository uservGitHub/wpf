using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Linq;
using System.Threading;
using Partical.DrawCore;

namespace System.Windows.DrawCore
{
    /// <summary>
    /// 小方格应用
    /// </summary>
    public class CellUtils
    {
        public static Pen RandomPen()
        {
            Random r = new Random((int)DateTime.Now.Ticks);            
            byte[] rbg = new byte[3];
            r.NextBytes(rbg);
            var color = Color.FromRgb(rbg[0], rbg[1], rbg[2]);
            Pen pen = new Pen(new SolidColorBrush(color), 4);
            return pen;
        }
        public static Pen ThePen(Color color)
        {
            Pen pen = new Pen(new SolidColorBrush(color), 4);
            return pen;
        }
        public static Color RandomColor()
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            byte[] rbg = new byte[3];
            r.NextBytes(rbg);
            var color = Color.FromRgb(rbg[0], rbg[1], rbg[2]);
            return color;
        }
        static CellUtils()
        {
            Width = 20;
            Height = 20;
        }
        public static UInt32 Width { get; internal set; }
        public static UInt32 Height { get; internal set; }
        /// <summary>
        /// 左上X
        /// </summary>
        public static UInt32 LocX { get; internal set; }
        /// <summary>
        /// 左上Y
        /// </summary>
        public static UInt32 LocY { get; internal set; }
        internal static DrawingContext Dc = null;
        internal static Brush DrawBrush = null;
        internal static Pen DrawPen = null;

        public static void DrawCell()
        {
            if (Dc != null)
            {
                Dc.DrawRectangle(DrawBrush, DrawPen,
                    new Rect(PartialConfig._ScaleX*LocX,
                    PartialConfig._ScaleY * LocY, 
                    PartialConfig._ScaleX * Width,
                    PartialConfig._ScaleY * Height));
            }
        }
    }
}

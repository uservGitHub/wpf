using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partical.DrawCore
{
    sealed class PartialConfig
    {
        //为了速度快，没有使用属性包装
        internal static double _ScaleX = 1.0;
        internal static double _ScaleY = 1.0;
        //internal static UInt32 _CalcLeft = 0;
        //internal static UInt32 _CalcTop = 0;
        //internal static UInt32 _CalcRight = 0;
        //internal static UInt32 _CalcBottom = 0;

        //internal static Action UpdateLeftMouseRect = () =>
        //{
        //    //更新右下点
        //    if(Mouse.LeftButton == MouseButtonState.Pressed)
        //    {

        //    }
        //    else
        //    {

        //    }
        //};

        static PartialConfig()
        {
            //DPI设置
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(System.IntPtr.Zero);
            var dipX = graphics.DpiX;
            var dipY = graphics.DpiY;
            graphics.Dispose();

            ToSolutionFactorX = dipX / 96;
            ToSolutionFactorY = dipY / 96;

            SolutionX = (int)(System.Windows.SystemParameters.PrimaryScreenWidth * ToSolutionFactorX);
            SolutionY = (int)(System.Windows.SystemParameters.PrimaryScreenHeight * ToSolutionFactorY);
        }

        #region 分辨率与像素
        /// <summary>
        /// 转换成分辨率的X值因子
        /// </summary>
        public static readonly double ToSolutionFactorX;
        /// <summary>
        /// 转换成分辨率的Y值因子
        /// </summary>
        public static readonly double ToSolutionFactorY;

        /// <summary>
        /// 分辨率X值
        /// </summary>
        public static readonly int SolutionX;
        /// <summary>
        /// 分辨率Y值
        /// </summary>
        public static readonly int SolutionY;
        #endregion

    }
}

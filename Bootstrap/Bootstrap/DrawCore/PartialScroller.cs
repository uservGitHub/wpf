using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Linq;

/// <summary>
/// Partical命名空间表示“某部分”，即某个类的一部分
/// Main表示输入主类，如主类中的XXX，这里就是MainXXX
/// 主类如何调用，这里（Partical）不管，只要不破坏Partical的结构即可。
/// Attach表示在Main中只装载一次
/// </summary>
namespace Partical.DrawCore
{
    using Dump = System.Diagnostics.Debug;
    /// <summary>
    /// 部分类的描述：Scroller，即它的功能
    /// MainXXX表示在主类中声明
    /// AttachXXX表示主类要加载或绑定XXX，即XXX要具有它的功能
    /// 额外是输出接口或伴随对象引用：
    /// 静态值采用PartialConfig.
    /// 输出用代理
    /// </summary>
    partial class PartialScroller
    {
        //滚动条
        ScrollViewer MainScroller = null;
        //滚动条的内容（可能是其孩子节点或后代节点）
        FrameworkElement MainFrame = null;
        //获取计算坐标系的大小
        Func<Tuple<UInt32, UInt32>> CalcCoordSysSizeFunc = null;
        //缩放后的操作
        Action<double, double> AfterScaledAction = null;

        #region 平移+缩放
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;
        Cursor beforMoveCursor;

        /// <summary>
        /// 装载滚动条
        /// </summary>
        /// <param name="target">面板（必须有大小）</param>
        public void AttachScroller(ScrollViewer scroller, FrameworkElement target, Action<double, double> afterScaleAction, Func<Tuple<UInt32, UInt32>> getCalcSizeFunc)
        {
            Dump.Assert(scroller != null && target != null && afterScaleAction != null && getCalcSizeFunc != null);

            MainScroller = scroller;
            MainFrame = target;
            AfterScaledAction = afterScaleAction;
            CalcCoordSysSizeFunc = getCalcSizeFunc;

            MainScroller.PreviewMouseRightButtonDown += OnMouseLeftButtonDown;
            MainScroller.PreviewMouseRightButtonUp += OnMouseLeftButtonUp;
            MainScroller.MouseRightButtonUp += OnMouseLeftButtonUp;
            MainScroller.MouseMove += OnMouseMove;

            MainScroller.PreviewMouseWheel += OnPreviewMouseWheel;
            MainScroller.ScrollChanged += OnScrollViewerScrollChanged;
        }

        #region 鼠标和滚轮事件
        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(MainScroller);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                MainScroller.ScrollToHorizontalOffset(MainScroller.HorizontalOffset - dX);
                MainScroller.ScrollToVerticalOffset(MainScroller.VerticalOffset - dY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(MainScroller);
            if (mousePos.X <= MainScroller.ViewportWidth && mousePos.Y < MainScroller.ViewportHeight)
            {
                beforMoveCursor = MainScroller.Cursor;
                MainScroller.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(MainScroller);
            }
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainScroller.Cursor = beforMoveCursor;
            MainScroller.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (lastMousePositionOnTarget.HasValue)
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(MainFrame);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / MainFrame.Width;
                    double multiplicatorY = e.ExtentHeight / MainFrame.Height;

                    double newOffsetX = MainScroller.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = MainScroller.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    MainScroller.ScrollToHorizontalOffset(newOffsetX);
                    MainScroller.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point pt1 = Mouse.GetPosition(MainScroller);
            Point pt2 = MainScroller.TranslatePoint(pt1, MainFrame);
            //Dump.Assert(pt2 == Mouse.GetPosition(MainFrame));
            if (e.Delta > 0)
            {
                //PartialConfig._Scale *= 1.1;
                PartialConfig._ScaleX *= 1.1;
                PartialConfig._ScaleY *= 1.1;
                var offsetX = pt2.X * 1.1 - pt1.X;
                var offsetY = pt2.Y * 1.1 - pt1.Y;
                MainScroller.ScrollToHorizontalOffset(offsetX);
                MainScroller.ScrollToVerticalOffset(offsetY);
            }
            else
            {
                //PartialConfig._Scale /= 1.1;
                PartialConfig._ScaleX /= 1.1;
                PartialConfig._ScaleY /= 1.1;
                var offsetX = pt2.X / 1.1 - pt1.X;
                var offsetY = pt2.Y / 1.1 - pt1.Y;
                MainScroller.ScrollToHorizontalOffset(offsetX);
                MainScroller.ScrollToVerticalOffset(offsetY);
            }

            var size = CalcCoordSysSizeFunc.Invoke();
            MainFrame.Width = size.Item1 * PartialConfig._ScaleX;
            MainFrame.Height = size.Item2 * PartialConfig._ScaleY;
            AfterScaledAction.Invoke(MainFrame.Width, MainFrame.Height);
            Dump.WriteLine(string.Format("Size=({0},{1})", MainFrame.Width, MainFrame.Height));
            e.Handled = true;
        }
        #endregion

        internal double ToCenterX;
        internal double ToCenterY;
        /// <summary>
        /// 定位到中心（中心点位于视区中心）
        /// </summary>
        /// <param name="calcCx">计算坐标系的X(double更精确)</param>
        /// <param name="calcCy">计算坐标系的Y(double更精确)</param>
        public void LocationCenter(double calcCx, double calcCy)
        {
            ToCenterX = calcCx;
            ToCenterY = calcCy;
            var Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                calcCx *= PartialConfig._ScaleX;
                calcCy *= PartialConfig._ScaleY;
                calcCx -= MainScroller.ViewportWidth / 2;
                calcCy -= MainScroller.ViewportHeight / 2;

                if (calcCx < 0) calcCx = 0;
                if (calcCy < 0) calcCy = 0;

                MainScroller.ScrollToHorizontalOffset(calcCx);
                MainScroller.ScrollToVerticalOffset(calcCy);
            }));
        }

        #endregion
    }
}

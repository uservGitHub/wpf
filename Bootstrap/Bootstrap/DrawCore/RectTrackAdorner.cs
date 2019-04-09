using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;

namespace Partical.DrawCore
{
    using Dump = System.Diagnostics.Debug;
    /// <summary>
    /// 矩形轨迹装饰器
    /// 只对左键有效
    /// 只由背景层触发
    /// 移动中的即时计算可由一个定时器来计算，即不移动时，计算。
    /// </summary>
    public class RectTrackAdorner : Adorner
    {
        #region 成员
        /// <summary>
        /// 起始点(左键按下，无方向)
        /// </summary>
        Point beginPt;
        /// <summary>
        /// 结束点
        /// </summary>
        Point? endPt;
        /// <summary>
        /// 层的宿主，看起来像是在其上的操作
        /// </summary>
        readonly UIElement host;
        //框选结束时操作(参数分别计算坐标系的是左、上、右、下且有序)
        Action<UInt32,UInt32,UInt32,UInt32> OpEndAction = null;
        //框选过程中的移动操作(参数分别计算坐标系的是左、上、右、下且有序)
        Action<UInt32, UInt32, UInt32, UInt32> OpMovingAction = null;
        /// <summary>
        /// 鼠标按键状态
        /// null：表示无按压
        /// true：表示按压左键
        /// false：表示按压右键
        /// </summary>
        private readonly bool? isLeftMouseDown;


        //鼠标左键和右键画线笔
        private static readonly Pen leftMousePen;
        //private static readonly Pen rightMousePen;
        #endregion


        #region 构造函数
        static RectTrackAdorner()
        {
            leftMousePen = new Pen(Brushes.Red, 1);
            leftMousePen.DashStyle = new DashStyle(new double[] { 8, 4, 0, 4 }, 1.5);
            leftMousePen.Freeze();

            //rightMousePen = new Pen(Brushes.Blue, 1);
            //rightMousePen.DashStyle = new DashStyle(new double[] { 8, 4, 0, 4 }, 1.5);
            //rightMousePen.Freeze();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">宿主</param>
        /// <param name="dragPt">h内的坐标</param>
        /// <param name="endAction"></param>
        /// <param name="movingAction"></param>
        public RectTrackAdorner(UIElement h, Point dragPt, Action<UInt32, UInt32, UInt32, UInt32> endAction, Action<UInt32, UInt32, UInt32, UInt32> movingAction)
            : base(h)
        {
            Dump.Assert(endAction != null && movingAction != null);
            //Dump.Assert(dragPt == Mouse.GetPosition(h));

            left = (UInt32)(dragPt.X / PartialConfig._ScaleX);
            top = (UInt32)(dragPt.Y / PartialConfig._ScaleY);

            host = h;
            beginPt = dragPt;
            OpEndAction = endAction;
            OpMovingAction = movingAction;

            if (Mouse.LeftButton == MouseButtonState.Pressed)
                isLeftMouseDown = true;
            else if (Mouse.RightButton == MouseButtonState.Pressed)
                isLeftMouseDown = false;
        }
        #endregion


        #region 函数
        /// <summary>
        /// 鼠标可以移动到客户区之外
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            var pt = e.GetPosition(this);

            //限定鼠标点在客户区内才处理

            if (pt.X < 0)
                pt.X = 0;
            if (pt.Y < 0)
                pt.Y = 0;
            if (pt.X > RenderSize.Width)
                pt.X = RenderSize.Width;
            if (pt.Y > RenderSize.Height)
                pt.Y = RenderSize.Height;

            //if (pt.X > 0 && pt.Y > 0 && pt.X < RenderSize.Width && pt.Y < RenderSize.Height)
            {
                if (!IsMouseCaptured)
                    CaptureMouse();

                //按压鼠标左键
                if (e.LeftButton == MouseButtonState.Pressed && isLeftMouseDown.Value)
                {
                    var dragPt = e.GetPosition(host);
                    right = (UInt32)(dragPt.X / PartialConfig._ScaleX);
                    bottom = (UInt32)(dragPt.Y / PartialConfig._ScaleY);
                    OpMovingAction.Invoke(left, top, right, bottom);
                }
                //else if (e.RightButton == MouseButtonState.Pressed && !isLeftMouseDown.Value) //按压鼠标右键
                //{
                //    UpdateRightSelection();
                //}

                endPt = pt;
                
                InvalidateVisual();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            //释放鼠标左键
            if (e.LeftButton == MouseButtonState.Released && isLeftMouseDown.Value)
            {
                var dragPt = e.GetPosition(host);
                right = (UInt32)(dragPt.X / PartialConfig._ScaleX);
                bottom = (UInt32)(dragPt.Y / PartialConfig._ScaleY);
                OpEndAction.Invoke(left, top, right, bottom);
            }
            //else if (e.RightButton == MouseButtonState.Released && !isLeftMouseDown.Value) //释放鼠标右键
            //{
            //    if (hostMouseUpAction != null)
            //        hostMouseUpAction();
            //}

            if (IsMouseCaptured)
                ReleaseMouseCapture();
            //释放鼠标时，框选轨迹消失
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(host);
            if (adornerLayer != null)
                adornerLayer.Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var brush = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
            //Brushes.Transparent
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
            if (endPt.HasValue && isLeftMouseDown != null)
            {
                if (isLeftMouseDown.Value)
                {
                    drawingContext.DrawRectangle(null, leftMousePen, new Rect(beginPt, endPt.Value));
                }
                //else
                //{
                //    drawingContext.DrawRectangle(null, rightMousePen, new Rect(beginPt, endPt.Value));
                //}
            }
        }
        #endregion
        UInt32 left, top, right, bottom;
    }
}

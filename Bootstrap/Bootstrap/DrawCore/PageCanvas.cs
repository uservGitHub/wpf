using System;
using System.Windows;
using System.Windows.Media;

namespace Partical.DrawCore
{
    using Dump = System.Diagnostics.Debug;

    /// <summary>
    /// 单页画布：
    /// 1、画布有大小
    /// 2、画布有Pop弹窗事件（按下时传递坐标，抬起时在坐标处弹窗）
    /// 3、画布可以作为控件放在容器中（比如滚动条中）
    /// 4、更多事件（平移、缩放）请在父容器中实现
    /// 5、画布是针对元素的（绘制、点击）
    /// 其他：
    /// 1、画布的大小是否有用（自动维护或触发其他事件）
    /// 2、设置大小可以撑起父控件，或者画布无大小，父控件有大小，如Grid（外层再加滚动条）
    /// 总结：
    /// 1、可以设置大小，且配合滚动条比较好，平移不刷新
    /// </summary>
    public class PageCanvas : FrameworkElement
    {
        #region 关键成员（状态控制、关键资源）
        /// <summary>
        /// 绘图句柄
        /// </summary>
        public DrawingContext _DrawHandle = null;
        /// <summary>
        /// 弹窗操作（使用单例更高效）
        /// </summary>
        public Action<PageRetType> _PopWindowAction = null;
        /// <summary>
        /// 高效绘图操作（无参为了速度）
        /// </summary>
        public Action _DrawAction = null;
        /// <summary>
        /// 选中操作（返回值是页返回类型）
        /// 参数1：测试点在画布中的坐标（元素命中测试）
        /// 参数2：标签项在屏幕中的坐标（弹出位置）
        /// </summary>
        public Func<Point, Point, PageRetType> _Selected = null;
        #endregion

        #region 临时变量（优化速度、无关结构）
        /// <summary>
        /// 鼠标按下时，选中的返回类型
        /// </summary>
        PageRetType selectedType = PageRetType.None;
        bool entryDown;
        private System.Diagnostics.Stopwatch Watch = null;
        private long LastTick = 0;
        #endregion

        #region 函数
        public PageCanvas()
        {
            Watch = new System.Diagnostics.Stopwatch();
            Watch.Start();
        }
        /// <summary>
        /// 呈现，无参更高效
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var handle = _DrawAction;
            if (handle != null)
            {
                _DrawHandle = drawingContext;
                PillTick("Rnd[");
                handle.Invoke();
                PillTick("]Rnd");
                _DrawHandle = null;
            }
        }
        /// <summary>
        /// 鼠标按下时的选中类型
        /// 一定会选中
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            entryDown = true;
            var handle = _Selected;
            if (handle != null)
            {
                selectedType = handle(this.PointFromScreen(new Point()), e.GetPosition(this));
            }
            PillTick("BtnDown");
            e.Handled = true;
        }
        /// <summary>
        /// 鼠标抬起时触发对应的弹窗
        /// 也可能没有
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!entryDown) return;

            if (_PopWindowAction != null && selectedType != PageRetType.None)
            {
                _PopWindowAction.Invoke(selectedType);
            }
            //清除
            selectedType = PageRetType.None;
            entryDown = false;
            PillTick("BtnUp");
            e.Handled = true;
        }
        #endregion

        [System.Diagnostics.Conditional("DEBUG")]
        private void PillTick(string tag)
        {
            var tick = Watch.ElapsedMilliseconds;
            var line = string.Format("{0}\t{1}\t{2}\t", tag, tick, tick-LastTick);
            LastTick = tick;
            Dump.Write(line);
        }
    }
}

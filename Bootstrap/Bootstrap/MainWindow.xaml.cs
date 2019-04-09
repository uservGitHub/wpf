using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.DrawCore;
using Partical.DrawCore;

namespace Bootstrap
{
    using Dump = System.Diagnostics.Debug;
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Action<DrawingContext> drawRectAction = null;
        private Brush drawBrush = null;
        private PartialScroller scroll = null;

        public MainWindow()
        {
            InitializeComponent();

            drawRectAction = (dc) =>
            {
                dc.DrawRectangle(drawBrush, null,
                    new Rect(10, 10, 200, 200));
            };
            page01._DrawAction = () =>
            {
                drawRectAction.Invoke(page01._DrawHandle);
            };
            
            Func<Tuple<UInt32, UInt32>> getSize = () =>
              {
                  return new Tuple<uint, uint>((UInt32)MaxCount * CellUtils.Width, (UInt32)MaxCount * CellUtils.Height);
              };
            var planeSize = getSize();
            page01.Width = planeSize.Item1;
            page01.Height = planeSize.Item2;

            scroll = new PartialScroller();
            scroll.AttachScroller(scroller01, page01, (a, b) =>
            {
                Dump.WriteLine("scale--");
                lineRTtoLB79_Click(null, null);
            },
            getSize
            );

            pageBackground._DrawAction = () =>
            {
                var size = getSize();
                var dc = pageBackground._DrawHandle;
                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255));
                dc.DrawRectangle(Brushes.Transparent, null,
                    new Rect(10, 10, size.Item1*PartialConfig._ScaleX, size.Item2 * PartialConfig._ScaleX));
            };
            pageBackground._Selected = (ptScreen, pt) =>
            {
                System.Windows.Documents.AdornerLayer layer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(page01);

                #region 轨迹面板
                if (layer != null)
                {
                    RectTrackAdorner adorner = new RectTrackAdorner(pageBackground, pt,
                        (a, b, c, d) =>
                        {
                            Dump.WriteLine("End");
                        },
                        (a, b, c, d) =>
                        {
                        });
                    if (adorner != null)
                        layer.Add(adorner);
                }
                #endregion
                return PageRetType.Record;
            };

        }
        int MaxCount = 9;
        private void lineRTtoLB79_Click(object sender, RoutedEventArgs e)
        {
            MaxCount += 50;
            //统一坐标系
            UInt32 x = 0;
            UInt32 y = 0;
            UInt32 w = 25;
            UInt32 h = 25;
            var pen = CellUtils.RandomPen();

            //x,y为起始点
            Action<DrawingContext, uint> drawLine = (dc, count) =>
            {
                CellUtils.DrawPen = pen;
                CellUtils.Width = w;
                CellUtils.Height = h;
                CellUtils.Dc = dc;

                //count是奇数，count项分两部分：
                //一部分竖排，一部分横排（往回）
                //中间项属于第一部分
                uint mid = (count + 1) / 2;
                for (uint i = 1; i <= mid; i++)//第一部分[1,mid]
                {
                    CellUtils.LocX = x;
                    CellUtils.LocY = y;
                    CellUtils.DrawCell();
                    y += h;
                }
                //第二部分[mid+1,count]
                y -= h;
                for (uint i = mid + 1; i <= count; i++)
                {
                    x -= w;
                    CellUtils.LocX = x;
                    CellUtils.LocY = y;
                    CellUtils.DrawCell();
                }
            };

            var target = page01;

            //target.Width = 79 * w;
            //target.Height = 79 * h;

            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                Color[] colors = new Color[(MaxCount - 1) / 2 + 1];
                for (int taskCount = 1, k = 0; taskCount <= MaxCount; taskCount += 2, k++)
                {
                    colors[k] = CellUtils.RandomColor();
                    int temp = taskCount;
                    Dispatcher.Invoke(() =>
                    {
                        target._DrawAction = () =>
                        {
                            for (uint i = 1, j = 0; i <= temp; i += 2, j++)
                            {
                                pen = CellUtils.ThePen(colors[j]);
                                x = 10 + j * w;
                                y = 10;
                                drawLine(page01._DrawHandle, i);
                            }
                        };
                        target.InvalidateVisual();
                    });
                    //System.Threading.Thread.Sleep(250);
                }
            });
            thread.Start();
        }
    }
}

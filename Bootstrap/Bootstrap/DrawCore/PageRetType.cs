using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partical.DrawCore
{
    /// <summary>
    /// 页返回类型（元素选中的类型）
    /// </summary>
    public enum PageRetType
    {
        /// <summary>
        /// 未选中
        /// </summary>
        None = 0,
        /// <summary>
        /// 单元格类型
        /// </summary>
        BitCell,
        /// <summary>
        /// 记录类型
        /// </summary>
        Record,
        /// <summary>
        /// 回调类型（执行用户代码）
        /// </summary>
        Hook,
    }
}

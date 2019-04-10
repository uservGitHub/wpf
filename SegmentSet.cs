using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tp.WpfControls.OnlyShow;

using Tp.Extension.TrackSide;
using Tp.Extension.ElectronicMap;

namespace Tp.Station
{
    public enum SegEndType:UInt16
    {
        无效 = 0,
        计轴 = 2,
        道岔 = 3,
    }
    public enum NoneType
    {
        None = 0,
        BegCut,
        EndCut,
        Segment,
        Luoji,
    }
    public class SpeedNode
    {
        //public Node(UInt16 speed, long begOffset, long endOffset)
        //{

        //}
        //public 
    }
    public class Node
    {
        public Node(NoneType type, UInt32 x, UInt32 y)
        {
            Type = type;
            X = x;
            Y = y;
        }
        public readonly UInt32 X;
        public readonly UInt32 Y;
        public readonly NoneType Type;
        public long Key
        {
            get
            {
                return ((long)X << 32) | Y;
            }
        }
    }
    /// <summary>
    /// 区段的定位
    /// </summary>
    public class SegPos
    {
        public readonly UInt16 SegId;
        public readonly int SegInd;
        //XBeg<XEnd
        public UInt32 XBeg { get; internal set; }
        public UInt32 XEnd { get; internal set; }
        public UInt32 YBeg { get; internal set; }
        public UInt32 YEnd { get; internal set; }
    }
    /// <summary>
    /// Segment数据单元
    /// </summary>
    public class DataSegmentUnit
    {
        internal static Func<DataSegmentUnit, bool, Tuple<UInt32, UInt32>> GetPtFun = null;

        #region 无需关心
        protected TabMemory memory;
        protected readonly int Pos;
        private DataSegmentUnit() { }
        private DataSegmentUnit(TabMemory mem, int index)
        {
            memory = mem;
            Ind = index;
            Pos = Config._LoopByteCount + Config._byteCount * Ind;            
            IndBeg = -1;
            IndEnd = -1;
            IndMainBeg = -1;
            IndMainEnd = -1;
            IndSideBeg = -1;
            IndSideEnd = -1;
            IndDaoChaBeg = -1;
            IndDaoChaEnd = -1;
            IndFigPosBeg = -1;
            IndFigLineBeg = -1;
            IndFigPosEnd = -1;
            IndFigLineEnd = -1;
            IndLianDongBeg = -1;
            IndLianDondEnd = -1;
            IndBaoHuJLH = -1;
            IndZhanTai = -1;
            IndJinLu = -1;
            IndWuLiSeg = -1;
            IndParkArea = -1;
            IsJinLuFrontLastToInd = -1;
            IsJinLuLastToInd = -1;
            IndJiZhouSegFromJinLu = -1;
            TagMessage = "";
            FixedSpeed = 0;
            SignalIndList = new List<int>();
            ParkPointIndList = new List<int>();
            BaliseIndList = new List<int>();
            JiZhouSegIndList = new List<int>();
            LuoJiSegIndList = new List<int>();
            ZxJinluIndList = new List<int>();
            FxJinluIndList = new List<int>();
            LuoJiInnerXList = new List<UInt32>();
            ZxTrigSignalIndList = new List<int>();
            FxTrigSignalIndList = new List<int>();
            IsHuiHe = false;
        }
        public static DataSegmentUnit Create(TabMemory mem, int index)
        {
            return new DataSegmentUnit(mem, index);
        }
        #endregion

        #region 与数据接口对应
        private static BitElectronicMapConfigGroup Config;
        private static BitElectronicMapConfig ConfId;
        private static BitElectronicMapConfig ConfLength;
        private static BitElectronicMapConfig ConfBegType;
        private static BitElectronicMapConfig ConfEndType;
        private static BitElectronicMapConfig ConfBegId;
        private static BitElectronicMapConfig ConfEndId;
        private static BitElectronicMapConfig ConfMainBegId;
        private static BitElectronicMapConfig ConfMainEndId;
        private static BitElectronicMapConfig ConfSideBegId;
        private static BitElectronicMapConfig ConfSideEndId;
        private static BitElectronicMapConfig ConfZcId;
        private static BitElectronicMapConfig ConfAtsId;
        private static BitElectronicMapConfig ConfCiId;
        private static BitElectronicMapConfig ConfLockX;
        private static BitElectronicMapConfig ConfAttrSpeed;
        #endregion

        #region 静态初始化，由DataUnitManager统一调用
        internal static void Init(BitElectronicMapConfigGroup config)
        {
            Config = config;
            ConfId = Config.First(s => s._FullName == "索引编号");
            ConfLength = Config.First(s => s._FullName == "长度（cm）");
            ConfBegType = Config.First(s => s._FullName == "起点端点类型");
            ConfEndType = Config.First(s => s._FullName == "终点端点类型");
            ConfBegId = Config.First(s => s._FullName == "起点端点编号");
            ConfEndId = Config.First(s => s._FullName == "终点端点编号");
            ConfMainBegId = Config.First(s => s._FullName == "起点正向相邻SegID");
            ConfMainEndId = Config.First(s => s._FullName == "终点正向相邻SegID");
            ConfSideBegId = Config.First(s => s._FullName == "起点侧向相邻SegID");
            ConfSideEndId = Config.First(s => s._FullName == "终点侧向相邻SegID");
            ConfZcId = Config.First(s => s._FullName == "所属ZC区域ID");
            ConfAtsId = Config.First(s => s._FullName == "所属ATS区域ID");
            ConfCiId = Config.First(s => s._FullName == "所属CI区域ID");
            ConfLockX = Config.First(s => s._FullName == "锁定长度");
            ConfAttrSpeed = Config.First(s => s._FullName == "SEG限速信息属性");
            //ConfAttrGradient = Config.First(s => s._FullName == "所属CI区域ID");
        }
        #endregion

        #region 数据接口
        public readonly int Ind;
        public bool IsHuiHe { get; internal set; }
        /// <summary>
        /// 国定限速
        /// </summary>
        public UInt16 FixedSpeed { get; internal set; }
        /// <summary>
        /// 固定限速的Km/h表示
        /// </summary>
        public UInt16 FixedKmHStyle { get; internal set; }
        public byte AttrSpeed
        {
            get { return memory.ReadByte(Pos + ConfAttrSpeed.BytePos); }
        }
        /// <summary>
        /// 是否休眠唤醒轨
        /// 有两对休眠唤醒应答器
        /// </summary>
        public bool CanXHG { get; internal set; }
        //public int X1 { get; internal set; }
        //public int Y1 { get; internal set; }
        //public int X2 { get; internal set; }
        //public int Y2 { get; internal set; }
        public string TagMessage { get; set; }
        public string TagWuLi { get; internal set; }
        public string TagLuoJi { get; internal set; }
        public string TagJiZhou { get; internal set; }
        public string TagSeg { get; internal set; }
        public FigFoldLine FoldLine { get; internal set; }
        //public int IndJiZhouSeg { get; internal set; }
        public int IndWuLiSeg { get; internal set; }
        public bool HasPM { get; internal set; }
        //是否折返区段
        //public bool HasZF { get; internal set; }
        /// <summary>
        /// 是否有车（来自仿真动力学模型）
        /// </summary>
        public bool IsVehicle { get; internal set; }
        /// <summary>
        /// 是否永久性故障
        /// </summary>
        public bool IsARB { get; internal set; }
        /// <summary>
        /// 临时变量，表示是否设置完毕
        /// </summary>
        public bool Flag { get; internal set; }
        /// <summary>
        /// 关联的进路下标
        /// </summary>
        public int IndJinLu { get; internal set; }
        public int IndJiZhouSegFromJinLu { get; internal set; }
        /// <summary>
        /// 是否为近路末端
        /// </summary>
        public int IsJinLuLastToInd { get; internal set; }
        /// <summary>
        /// 是否为近路末端
        /// </summary>
        public int IsJinLuFrontLastToInd { get; internal set; }
        /// <summary>
        /// 关联的保护区段进路号
        /// </summary>
        public int IndBaoHuJLH { get; internal set; }
        public int IndParkArea { get; internal set; }
        public int IndZhanTai { get; internal set; }
        public int IndFigLineEnd { get; internal set; }
        public int IndFigPosEnd { get; internal set; }
        public int IndFigLineBeg { get; internal set; }
        public int IndFigPosBeg { get; internal set; }

        public int IndLianDongBeg { get; internal set; }
        public int IndLianDondEnd { get; internal set; }

        public int IndBeg { get; internal set; }
        public int IndEnd { get; internal set; }
        public int IndMainBeg { get; internal set; }
        public int IndMainEnd { get; internal set; }
        public int IndSideBeg { get; internal set; }
        public int IndSideEnd { get; internal set; }
        public int IndDaoChaBeg { get; internal set; }
        public int IndDaoChaEnd { get; internal set; }
        public IList<int> ParkPointIndList { get; private set; }
        public IList<int> SignalIndList { get; private set; }
        public IList<int> BaliseIndList { get; private set; }
        public IList<int> JiZhouSegIndList { get; private set; }
        public IList<int> LuoJiSegIndList { get; private set; }
        public IList<UInt32> LuoJiInnerXList { get; private set; }  //内置的逻辑区段X分界
        public IList<int> ZxJinluIndList { get; private set; }
        public IList<int> FxJinluIndList { get; private set; }
        public IList<int> ZxTrigSignalIndList { get; private set; } //目前的数据只有一个
        public IList<int> FxTrigSignalIndList { get; private set; } //目前的数据只有一个
        public UInt32 XBeg { get; internal set; }
        public UInt32 YBeg { get; internal set; }
        public UInt32 XEnd { get; internal set; }
        public UInt32 YEnd { get; internal set; }
        public Tuple<UInt32, UInt32> XYBeg
        {
            get { return GetPtFun(this, true); }
        }
        public Tuple<UInt32, UInt32> XYEnd
        {
            get { return GetPtFun(this, false); }
        }

        

        //public void DrawDebug()
        //{
        //    if (DataUnitManager._DrawingContext != null)
        //        ViewConfig.DrawSeg(XYBeg, IsMainBegCut, XYEnd, IsMainEndCut, 
        //            string.Format("{0}_{1}", Id,Length/100), 
        //            DataUnitManager._DrawingContext);
        //}
        /// <summary>
        /// 正线起点断头
        /// </summary>
        public bool IsMainBegCut
        {
            get { return MainBegId == DataUnitManager.NoneId; }
        }
        /// <summary>
        /// 正线终点断尾
        /// </summary>
        public bool IsMainEndCut
        {
            get { return MainEndId == DataUnitManager.NoneId; }
        }
        /// <summary>
        /// 侧向接入
        /// </summary>
        public bool HasSideIn
        {
            get { return SideBegId != DataUnitManager.NoneId; }
        }
        /// <summary>
        /// 侧向引出
        /// </summary>
        public bool HasSideOut
        {
            get { return SideEndId != DataUnitManager.NoneId; }
        }
        public UInt16 Id
        {
            get { return memory.ReadUInt16(Pos + ConfId.BytePos); }
        }
        public UInt32 Length
        {
            get { return memory.ReadUInt32(Pos + ConfLength.BytePos); }
        }
        public SegEndType BegType
        {
            get { return (SegEndType)memory.ReadUInt16(Pos + ConfBegType.BytePos); }
        }
        public SegEndType EndType
        {
            get { return (SegEndType)memory.ReadUInt16(Pos + ConfEndType.BytePos); }
        }
        public UInt16 BegId
        {
            get { return memory.ReadUInt16(Pos + ConfBegId.BytePos); }
        }
        public UInt16 EndId
        {
            get { return memory.ReadUInt16(Pos + ConfEndId.BytePos); }
        }
        public UInt16 MainBegId
        {
            get { return memory.ReadUInt16(Pos + ConfMainBegId.BytePos); }
        }
        public UInt16 MainEndId
        {
            get { return memory.ReadUInt16(Pos + ConfMainEndId.BytePos); }
        }
        public UInt16 SideBegId
        {
            get { return memory.ReadUInt16(Pos + ConfSideBegId.BytePos); }
        }
        public UInt16 SideEndId
        {
            get { return memory.ReadUInt16(Pos + ConfSideEndId.BytePos); }
        }
        public UInt16 AtsId
        {
            get { return memory.ReadUInt16(Pos + ConfAtsId.BytePos); }
        }
        public UInt16 ZcId
        {
            get { return memory.ReadUInt16(Pos + ConfZcId.BytePos); }
        }
        public UInt16 CiId
        {
            get { return memory.ReadUInt16(Pos + ConfCiId.BytePos); }
        }
        /// <summary>
        /// 相对起点的偏移
        /// </summary>
        public string LockX
        {
            get { return ConfLockX.ConvertToString(memory.SubArray(Pos + ConfLockX.BytePos, ConfLockX.TypeByteCount)); }
        }
        #endregion
    }

    /// <summary>
    /// Segment数据集合
    /// </summary>
    public class DataSegmentCollection : IEnumerable<DataSegmentUnit>
    {
        #region 扩展
        private List<SegPos> segPosList;
        internal void OrderSegPos()
        {
            if (segPosList != null) return;
            segPosList = new List<SegPos>();
            //排序
            //...
        }
        public DataSegmentUnit ItemFromRect(UInt32 left, UInt32 top, UInt32 right, UInt32 bottom)
        {
            //查找Segment的交集
            return null;
        }
        #endregion
        private DataSegmentCollection() { }
        private List<DataSegmentUnit> dataList;
        protected TabMemory memory;
        public DataSegmentCollection(TabMemory mem)
        {
            this.memory = mem;
            dataList = new List<DataSegmentUnit>();
            for (int i = 0; i < Count; i++)
            {
                dataList.Add(DataSegmentUnit.Create(memory, i));
            }
        }
        /// <summary>
        /// 索引访问
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataSegmentUnit this[int index]
        {
            get { return dataList[index]; }
        }
        /// <summary>
        /// 由Id获得子项，二分法查找
        /// </summary>
        /// <param name="id">项的Id</param>
        /// <returns>不存在，返回null</returns>
        public DataSegmentUnit ItemFromId(UInt16 id)
        {
            int low = 0;
            int high = dataList.Count - 1;
            while (low <= high)
            {
                int middle = (low + high) / 2;
                var item = this[middle];
                var value = item.Id;
                if (value == id) return item;
                else if (value > id) high = middle - 1;
                else low = middle + 1;
            }
            return null;
        }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count
        {
            get
            {
                return memory.ReadUInt16(0);
            }
        }
        /// <summary>
        /// 输出字节流
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return memory.ToArray();
        }
        #region IEnumerable 接口
        public IEnumerator<DataSegmentUnit> GetEnumerator()
        {
            if (memory == null || Count == 0)
                yield break;

            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}

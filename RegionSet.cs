using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Configuration;
using System.Windows;
using System.Text;
using Tp.WpfControls.OnlyShow;

using Tp.Extension.TrackSide;
using Tp.Extension.ElectronicMap;

namespace Tp.Station
{
    public enum RegionType:UInt16
    {
        无效 = 0,
        CI = 0x3c,
        ZC = 0x1e,
        ATS = 3,
    }
    public enum RegionAttr : Byte
    {
        无效 = 0,
        正线 = 1,
        试车线 = 2,
        停车场 = 3,
        车辆段 = 4,
    }
    /// <summary>
    /// 道岔数据单元
    /// </summary>
    public class DataRegionUnit
    {
        #region 无需关心
        protected TabMemory memory;
        protected readonly int Pos;
        private DataRegionUnit() { }
        private DataRegionUnit(TabMemory mem, int index)
        {
            memory = mem;
            Ind = index;
            Pos = Config._LoopByteCount + Config._byteCount * Ind;
            DeviceId = (UInt16)((((UInt16)Type) << 8) | TargetId);
        }
        public static DataRegionUnit Create(TabMemory mem, int index)
        {
            return new DataRegionUnit(mem, index);
        }
        #endregion

        #region 与数据接口对应
        private static BitElectronicMapConfigGroup Config;
        private static BitElectronicMapConfig ConfId;
        private static BitElectronicMapConfig ConfType;
        private static BitElectronicMapConfig ConfAttr;
        private static BitElectronicMapConfig ConfTargetId;
        private static BitElectronicMapConfig ConfZxNextId;
        private static BitElectronicMapConfig ConfFxNextId;
        #endregion

        #region 静态初始化，由DataUnitManager统一调用
        internal static void Init(BitElectronicMapConfigGroup config)
        {
            Config = config;
            ConfId = Config.First(s => s._FullName == "索引编号");
            ConfType = Config.First(s => s._FullName == "区域类型");
            ConfAttr = Config.First(s => s._FullName == "区域属性");
            ConfTargetId = Config.First(s => s._FullName == "区域ID");
            ConfZxNextId = Config.First(s => s._FullName == "正向NextID");
            ConfFxNextId = Config.First(s => s._FullName == "反向NextID");
        }
        #endregion

        #region 数据接口
        public readonly int Ind;
        public readonly UInt16 DeviceId;

        public UInt16 Id
        {
            get { return memory.ReadUInt16(Pos + ConfId.BytePos); }
        }
        public UInt16 ZxNextId
        {
            get { return memory.ReadUInt16(Pos + ConfZxNextId.BytePos); }
        }
        public UInt16 FxNextId
        {
            get { return memory.ReadUInt16(Pos + ConfFxNextId.BytePos); }
        }
        public UInt16 TargetId
        {
            get { return memory.ReadUInt16(Pos + ConfTargetId.BytePos); }
        }
        public RegionAttr Attr
        {
            get { return (RegionAttr)memory.ReadUInt16(Pos + ConfAttr.BytePos); }
        }
        public RegionType Type
        {
            get { return (RegionType)(memory.ReadUInt16(Pos + ConfType.BytePos)); }
        }
        #endregion
    }

    /// <summary>
    /// 道岔数据集合
    /// </summary>
    public class DataRegionCollection : IEnumerable<DataRegionUnit>
    {
        private DataRegionCollection() { }
        private List<DataRegionUnit> dataList;
        protected TabMemory memory;
        public DataRegionCollection(TabMemory mem)
        {
            this.memory = mem;
            dataList = new List<DataRegionUnit>();
            for (int i = 0; i < Count; i++)
            {
                dataList.Add(DataRegionUnit.Create(memory, i));
            }
        }
        /// <summary>
        /// 索引访问
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataRegionUnit this[int index]
        {
            get { return dataList[index]; }
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
        public IEnumerator<DataRegionUnit> GetEnumerator()
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

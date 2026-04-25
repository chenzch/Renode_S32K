using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Core;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Peripherals;
using WT.Renode;

namespace Antmicro.Renode.Peripherals.NXP.S32K3
{
    // 自定义外设：32 位寄存器映射的简单设备
    public class MCResetGenerationModule : IPeripheral, IDoubleWordPeripheral, IKnownSize
    {
        // 寄存器偏移定义（相对基地址，非绝对）
        private enum Registers : long
        {
            Control = 0x00,   // 控制寄存器
            Data = 0x04,      // 数据寄存器
            Status = 0x08     // 状态寄存器
        }

        // 寄存器字段（使用 Register Framework 简化管理）
        private readonly DoubleWordRegisterCollection _registers;

        public MCResetGenerationModule(IMachine machine, string registersFileRoot)
        {
            _registers = new DoubleWordRegisterCollection(this);
            // 定义寄存器与字段
            DefineRegisters(registersFileRoot);
            // 初始复位
            Reset();
        }

        // 实现寄存器定义（推荐用 Register Framework）
        private void DefineRegisters(string resetValue)
        {
            ResetValue value = ResetValue.Load(resetValue);
            // 控制寄存器：bit0 使能，bit1 中断使能
            _registers.DefineRegister((long)Registers.Control, resetValue: value.Get("MC_RGM.DES", 0x01, this.Log))
                .WithFlag(0, out _DES_F_POR, name: "F_POR", writeCallback: (oldVal, newVal) => {_DES_F_POR.Value = (newVal ? false : oldVal);})
                .WithFlag(3, out _DES_FCCU_FTR, name: "FCCU_FTR", writeCallback: (oldVal, newVal) => {_DES_FCCU_FTR.Value = (newVal ? false : oldVal);})
                .WithFlag(4, out _DES_STCU_URF, name: "STCU_URF", writeCallback: (oldVal, newVal) => {_DES_STCU_URF.Value = (newVal ? false : oldVal);})
                .WithFlag(6, out _DES_MC_RGM_FRE, name: "MC_RGM_FRE", writeCallback: (oldVal, newVal) => {_DES_MC_RGM_FRE.Value = (newVal ? false : oldVal);})
                .WithFlag(8, out _DES_FXOSC_FAIL, name: "FXOSC_FAIL", writeCallback: (oldVal, newVal) => {_DES_FXOSC_FAIL.Value = (newVal ? false : oldVal);})
                .WithFlag(9, out _DES_PLL_LOL, name: "PLL_LOL", writeCallback: (oldVal, newVal) => {_DES_PLL_LOL.Value = (newVal ? false : oldVal);})
                .WithFlag(10, out _DES_CORE_CLK_FAIL, name: "CORE_CLK_FAIL", writeCallback: (oldVal, newVal) => {_DES_CORE_CLK_FAIL.Value = (newVal ? false : oldVal);})
                .WithFlag(12, out _DES_AIPS_PLAT_CLK_FAIL, name: "AIPS_PLAT_CLK_FAIL", writeCallback: (oldVal, newVal) => {_DES_AIPS_PLAT_CLK_FAIL.Value = (newVal ? false : oldVal);})
                .WithFlag(14, out _DES_HSE_CLK_FAIL, name: "HSE_CLK_FAIL", writeCallback: (oldVal, newVal) => {_DES_HSE_CLK_FAIL.Value = (newVal ? false : oldVal);})
                .WithFlag(15, out _DES_SYS_DIV_FAIL, name: "SYS_DIV_FAIL", writeCallback: (oldVal, newVal) => {_DES_SYS_DIV_FAIL.Value = (newVal ? false : oldVal);})
                .WithFlag(16, out _DES_CM7_CORE_CLK_FAIL, name: "CM7_CORE_CLK_FAIL", writeCallback: (oldVal, newVal) => {_DES_CM7_CORE_CLK_FAIL.Value = (newVal ? false : oldVal);})
                .WithFlag(17, out _DES_HSE_TMPR_RST, name: "HSE_TMPR_RST", writeCallback: (oldVal, newVal) => {_DES_HSE_TMPR_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(18, out _DES_HSE_SNVS_RST, name: "HSE_SNVS_RST", writeCallback: (oldVal, newVal) => {_DES_HSE_SNVS_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(29, out _DES_SW_DEST, name: "SW_DEST", writeCallback: (oldVal, newVal) => {_DES_SW_DEST.Value = (newVal ? false : oldVal);})
                .WithFlag(30, out _DES_DEBUG_DEST, name: "DEBUG_DEST", writeCallback: (oldVal, newVal) => {_DES_DEBUG_DEST.Value = (newVal ? false : oldVal);});

            // 数据寄存器：可读可写
            _registers.DefineRegister((long)Registers.Data, resetValue: 0x00)
                .WithValueField(0, 32, out _dataField, name: "DATA",
                    writeCallback: (oldVal, newVal) => _dataBuffer = (uint)newVal);

            // 状态寄存器：bit0 忙，bit1 中断标志
            _registers.DefineRegister((long)Registers.Status, resetValue: 0x00)
                .WithFlag(0, out _busyBit, name: "BUSY")
                .WithFlag(1, out _interruptFlagBit, name: "INTERRUPT_FLAG",
                    writeCallback: (oldVal, newVal) =>
                    {
                        if (newVal) _interruptFlagBit.Value = false; // 写 0 清中断
                    });
        }

        // ---------------- 接口实现 ----------------
        // 32 位读操作
        uint IDoubleWordPeripheral.ReadDoubleWord(long offset)
        {
            return _registers.Read(offset);
        }

        // 32 位写操作
        void IDoubleWordPeripheral.WriteDoubleWord(long offset, uint value)
        {
            _registers.Write(offset, value);
        }

        // 复位逻辑
        public void Reset()
        {
            _registers.Reset();
            _dataBuffer = 0x00;
            _busyBit.Value = false;
            _interruptFlagBit.Value = false;
            this.Log(LogLevel.Info, "MCResetGenerationModule 已复位");
        }

        // 外设大小（必须实现 IKnownSize）
        public long Size => 0x28; // 占用 40 字节（3 个寄存器共 12 字节，对齐到 16）

        // ---------------- 私有字段 ----------------
        private IFlagRegisterField _DES_F_POR;
        private IFlagRegisterField _DES_FCCU_FTR;
        private IFlagRegisterField _DES_STCU_URF;
        private IFlagRegisterField _DES_MC_RGM_FRE;
        private IFlagRegisterField _DES_FXOSC_FAIL;
        private IFlagRegisterField _DES_PLL_LOL;
        private IFlagRegisterField _DES_CORE_CLK_FAIL;
        private IFlagRegisterField _DES_AIPS_PLAT_CLK_FAIL;
        private IFlagRegisterField _DES_HSE_CLK_FAIL;
        private IFlagRegisterField _DES_SYS_DIV_FAIL;
        private IFlagRegisterField _DES_CM7_CORE_CLK_FAIL;
        private IFlagRegisterField _DES_HSE_TMPR_RST;
        private IFlagRegisterField _DES_HSE_SNVS_RST;
        private IFlagRegisterField _DES_SW_DEST;
        private IFlagRegisterField _DES_DEBUG_DEST;

        private IValueRegisterField _dataField;
        private IFlagRegisterField _busyBit;
        private IFlagRegisterField _interruptFlagBit;
        private uint _dataBuffer;
    }
}

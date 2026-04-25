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
            _registers.DefineRegister(0x00, resetValue: value.Get("MC_RGM.DES", 0x01, this.Log))
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

            _registers.DefineRegister(0x08, resetValue: value.Get("MC_RGM.FES", 0x00, this.Log))
                .WithFlag(0, out _FES_F_EXR, name: "F_EXR", writeCallback: (oldVal, newVal) => {_FES_F_EXR.Value = (newVal ? false : oldVal);})
                .WithFlag(3, out _FES_FCCU_RST, name: "FCCU_RST", writeCallback: (oldVal, newVal) => {_FES_FCCU_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(4, out _FES_ST_DONE, name: "ST_DONE", writeCallback: (oldVal, newVal) => {_FES_ST_DONE.Value = (newVal ? false : oldVal);})
                .WithFlag(6, out _FES_SWT0_RST, name: "SWT0_RST", writeCallback: (oldVal, newVal) => {_FES_SWT0_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(7, out _FES_SWT1_RST, name: "SWT1_RST", writeCallback: (oldVal, newVal) => {_FES_SWT1_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(8, out _FES_SWT2_RST, name: "SWT2_RST", writeCallback: (oldVal, newVal) => {_FES_SWT2_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(9, out _FES_JTAG_RST, name: "JTAG_RST", writeCallback: (oldVal, newVal) => {_FES_JTAG_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(10, out _FES_SWT3_RST, name: "SWT3_RST", writeCallback: (oldVal, newVal) => {_FES_SWT3_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(12, out _FES_PLL_AUX, name: "PLL_AUX", writeCallback: (oldVal, newVal) => {_FES_PLL_AUX.Value = (newVal ? false : oldVal);})
                .WithFlag(16, out _FES_HSE_SWT_RST, name: "HSE_SWT_RST", writeCallback: (oldVal, newVal) => {_FES_HSE_SWT_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(20, out _FES_HSE_BOOT_RST, name: "HSE_BOOT_RST", writeCallback: (oldVal, newVal) => {_FES_HSE_BOOT_RST.Value = (newVal ? false : oldVal);})
                .WithFlag(29, out _FES_SW_FUNC, name: "SW_FUNC", writeCallback: (oldVal, newVal) => {_FES_SW_FUNC.Value = (newVal ? false : oldVal);})
                .WithFlag(30, out _FES_DEBUG_FUNC, name: "DEBUG_FUNC", writeCallback: (oldVal, newVal) => {_FES_DEBUG_FUNC.Value = (newVal ? false : oldVal);});


            // 数据寄存器：可读可写
            _registers.DefineRegister(0x04, resetValue: 0x00)
                .WithValueField(0, 32, out _dataField, name: "DATA",
                    writeCallback: (oldVal, newVal) => _dataBuffer = (uint)newVal);
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

        private IFlagRegisterField _FES_F_EXR;
        private IFlagRegisterField _FES_FCCU_RST;
        private IFlagRegisterField _FES_ST_DONE;
        private IFlagRegisterField _FES_SWT0_RST;
        private IFlagRegisterField _FES_SWT1_RST;
        private IFlagRegisterField _FES_SWT2_RST;
        private IFlagRegisterField _FES_JTAG_RST;
        private IFlagRegisterField _FES_SWT3_RST;
        private IFlagRegisterField _FES_PLL_AUX;
        private IFlagRegisterField _FES_HSE_SWT_RST;
        private IFlagRegisterField _FES_HSE_BOOT_RST;
        private IFlagRegisterField _FES_SW_FUNC;
        private IFlagRegisterField _FES_DEBUG_FUNC;

        private IValueRegisterField _dataField;
        private uint _dataBuffer;
    }
}

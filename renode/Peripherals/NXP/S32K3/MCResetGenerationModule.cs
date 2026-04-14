using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Core;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Peripherals;

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

        public MCResetGenerationModule(IMachine machine)
        {
            _registers = new DoubleWordRegisterCollection(this);
            // 定义寄存器与字段
            DefineRegisters();
            // 初始复位
            Reset();
        }

        // 实现寄存器定义（推荐用 Register Framework）
        private void DefineRegisters()
        {
            // 控制寄存器：bit0 使能，bit1 中断使能
            _registers.DefineRegister((long)Registers.Control, resetValue: 0x01)
                .WithFlag(0, out _enableBit, name: "ENABLE")
                .WithFlag(1, out _interruptEnableBit, name: "INTERRUPT_ENABLE");

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
        private IFlagRegisterField _enableBit;
        private IFlagRegisterField _interruptEnableBit;
        private IValueRegisterField _dataField;
        private IFlagRegisterField _busyBit;
        private IFlagRegisterField _interruptFlagBit;
        private uint _dataBuffer;
    }
}

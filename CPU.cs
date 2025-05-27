using static NestedSharp.InstructionType;

namespace NestedSharp;

/// <summary>
///     Emulation of the 6502 CPU inside a NES Console.
/// </summary>
public class CPU
{
    [Flags]
    public enum CpuFlags : byte
    {
        Carry = 0b0000_0001,
        Zero = 0b0000_0010,
        InterruptDisable = 0b0000_0100,
        DecimalMode = 0b0000_1000,
        BreakCommand = 0b0001_0000,
        Unused = 0b0010_0000,
        Overflow = 0b0100_0000,
        Negative = 0b1000_0000
    }

    private readonly byte[] memory = new byte[65536];
    private byte programCounter;
    private byte register_a;
    private byte register_x;
    private byte register_y;
    private byte stackPointer;
    private byte status;

    public ushort get_operand_address(AddressingMode addressingMode)
    {
        return addressingMode switch
        {
            AddressingMode.Immediate => programCounter,
            AddressingMode.ZeroPage => mem_read(programCounter),
            AddressingMode.ZeroPageX => (ushort)(mem_read(programCounter) + register_x),
            AddressingMode.ZeroPageY => (ushort)(mem_read(programCounter) + register_y),
            AddressingMode.Absolute => mem_read_u16(programCounter),
            AddressingMode.AbsoluteX => (ushort)(mem_read_u16(programCounter) + register_x),
            AddressingMode.AbsoluteY => (ushort)(mem_read_u16(programCounter) + register_y),
            AddressingMode.IndirectX => IndirectX(),
            AddressingMode.IndirectY => IndirectY(),
            AddressingMode.Indirect => mem_read_u16(programCounter),
            AddressingMode.Implied => throw new Exception("Implied addressing mode does not use an operand."),
            AddressingMode.Accumulator => throw new Exception("Accumulator addressing mode does not use an operand.")
        };
    }

    private ushort IndirectY()
    {
        var a = mem_read(programCounter);
        var low = mem_read(a);
        var high = mem_read((byte)(a + 1));
        var ptr = (ushort)((high << 8) | low);
        return (ushort)(ptr + register_y);
    }

    private ushort IndirectX()
    {
        var a = mem_read(programCounter);
        var ptr = (byte)(a + register_x);
        var low = mem_read(ptr);
        var high = mem_read((byte)(ptr + 1));
        return (ushort)((high << 8) | low);
    }

    public byte mem_read(ushort address)
    {
        return memory[address];
    }

    public void mem_write(ushort address, byte value)
    {
        memory[address] = value;
    }

    public ushort mem_read_u16(ushort address)
    {
        ushort low = mem_read(address);
        ushort high = mem_read((ushort)(address + 1));
        return (ushort)((high << 8) | low);
    }

    public void mem_write_u16(ushort address, ushort data)
    {
        var low = (byte)(data & 0xff);
        var high = (byte)((data >> 8) & 0xff);
        mem_write(address, low);
        mem_write((ushort)(address + 1), high);
    }

    public void LoadAndRun(byte[] program)
    {
        load(program);
        reset();
        run();
    }

    public void load(byte[] program)
    {
        program.CopyTo(memory, 0x8000);
        mem_write_u16(0x8000, 0xFFFC);
    }

    public void reset()
    {
        programCounter = mem_read(0xFFFC);
        stackPointer = 0;
        status = 0;
        register_a = 0;
        register_x = 0;
        register_y = 0;
    }

    public void run()
    {
        while (true)
        {
            var opcode = CpuOpcodes.OpcodesDict[mem_read(programCounter)];
            switch (opcode.mnemonic)
            {
                case BRK:
                    return;
                case LDA:
                    LoadAccumulator(opcode.addressingMode);
                    break;
                case TAX:
                    TransferAccumulatorToX();
                    break;
                case INX:
                    IncrementX();
                    break;
                case TAY:
                    TransferAccumulatorToY();
                    break;
                case INY:
                    IncrementY();
                    break;
                case ADC:
                    AddWithCarry(opcode.addressingMode);
                    break;
                case INC:
                    IncrementMemory(opcode.addressingMode);
                    break;
                case JMP:
                    Jump(opcode.addressingMode);
                    break;
                case NOP:
                    break;

                default:
                    throw new Exception("Todo or invalid.");
                    break;
            }

            programCounter += opcode.length;
        }
    }


    public void UpdateZeroFlag(byte value)
    {
        if (value == 0)
            SetFlag(CpuFlags.Zero);
        else
            ClearFlag(CpuFlags.Zero);
    }

    private void UpdateStatusNegativeFlag(byte value)
    {
        if ((value & 0b1000_0000) != 0)
            SetFlag(CpuFlags.Negative);
        else
            ClearFlag(CpuFlags.Negative);
    }

    private void UpdateStatusCarryFlag(bool overflow)
    {
        if (overflow)
            status |= 0b0000_0001;
        else
            status &= 0b1111_1110;
    }

    public void ClearFlag(CpuFlags flag)
    {
        status &= (byte)~(byte)flag;
    }

    public void SetFlag(CpuFlags flag)
    {
        status |= (byte)flag;
    }

    public bool IsFlagSet(CpuFlags flag)
    {
        return (status & (byte)flag) != 0;
    }

    #region Opcode_Implementations

    public void LoadAccumulator(AddressingMode addressingMode)
    {
        var address = get_operand_address(addressingMode);
        var value = mem_read(address);
        register_a = value;
        UpdateZeroFlag(register_a);
        UpdateStatusNegativeFlag(register_a);
    }

    public void TransferAccumulatorToX()
    {
        register_x = register_a;
        UpdateZeroFlag(register_x);
        UpdateStatusNegativeFlag(register_x);
    }

    public void IncrementMemory(AddressingMode addressingMode)
    {
        var value = mem_read(get_operand_address(addressingMode));
        value++;
        mem_write(get_operand_address(addressingMode), value);
        UpdateStatusNegativeFlag(value);
        UpdateZeroFlag(value);
    }

    public void TransferAccumulatorToY()
    {
        register_y = register_a;
        UpdateZeroFlag(register_y);
        UpdateStatusNegativeFlag(register_y);
    }

    public void IncrementX()
    {
        register_y = (byte)(register_x + 1);
        UpdateStatusNegativeFlag(register_x);
        UpdateZeroFlag(register_x);
    }

    public void IncrementY()
    {
        register_y = (byte)(register_y + 1);
        UpdateStatusNegativeFlag(register_y);
        UpdateZeroFlag(register_y);
    }

    public void AddWithCarry(AddressingMode mode)
    {
        var value = mem_read(get_operand_address(mode));
        var carry = status & 0b0000_0001;
        var a = register_a + value + carry;
        register_a = (byte)a;
        UpdateZeroFlag(register_a);
        UpdateStatusNegativeFlag(register_a);
        if ((a & 0b11110000) != 0) UpdateStatusCarryFlag(true);
    }

    public void AndWithAccumulator(AddressingMode mode)
    {
        var value = mem_read(get_operand_address(mode));
        register_a &= value;
        UpdateZeroFlag(register_a);
        UpdateStatusNegativeFlag(register_a);
    }

    public void ArithmeticShiftLeft(AddressingMode mode)
    {
        byte value;
        value = mode == AddressingMode.Accumulator ? register_a : mem_read(get_operand_address(mode));
        // Shift the value left. Bit 0 gets set to 0.
        var shiftedValue = (byte)(value << 1);

        // Update memory with shifted value
        if (mode == AddressingMode.Accumulator)
            register_a = shiftedValue;
        else
            mem_write(get_operand_address(mode), shiftedValue);

        // Update status flags
        UpdateZeroFlag(shiftedValue);
        UpdateStatusNegativeFlag(shiftedValue);

        // Set the Carry Flag based on bit 7 of the original value
        if ((value & 0b1000_0000) != 0)
            status |= (byte)CpuFlags.Carry;
        else
            status &= (byte)~CpuFlags.Carry;
    }

    public void BranchifCarryClear(AddressingMode mode)
    {
        //if carry flag is clear

        //next value is treated as a signed byte.
        var unsigned = mem_read(get_operand_address(AddressingMode.Immediate));
        var signed = (sbyte)unsigned;
        //add the signed byte to the program counter's value.
        var res = programCounter + signed;
        // programCounter = (byte)res;
    }

    private void Jump(AddressingMode addressingMode)
    {
        switch (addressingMode)
        {
            case AddressingMode.Absolute:
                //Jumps to the address specified in the next byte.
                programCounter = mem_read(get_operand_address(addressingMode));
                break;
            case AddressingMode.Indirect:

                break;
        }
    }

    public void CLD()
    {
    }

    #endregion
}
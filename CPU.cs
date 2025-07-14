using static NestedSharp.InstructionType;

namespace NestedSharp;

/// <summary>
///     Emulation of the 6502 CPU inside a NES Console.
/// </summary>
public class Cpu
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

	private readonly byte[] _memory = new byte[65536];
	private byte _programCounter;
	private byte _registerA;
	private byte _registerX;
	private byte _registerY;
	private byte _stackPointer;
	private byte _status;

	public ushort get_operand_address(AddressingMode addressingMode)
	{
		return addressingMode switch
		{
			AddressingMode.Immediate => _programCounter,
			AddressingMode.ZeroPage => mem_read(_programCounter),
			AddressingMode.ZeroPageX => (ushort)(mem_read(_programCounter) + _registerX),
			AddressingMode.ZeroPageY => (ushort)(mem_read(_programCounter) + _registerY),
			AddressingMode.Absolute => mem_read_u16(_programCounter),
			AddressingMode.AbsoluteX => (ushort)(mem_read_u16(_programCounter) + _registerX),
			AddressingMode.AbsoluteY => (ushort)(mem_read_u16(_programCounter) + _registerY),
			AddressingMode.IndirectX => IndirectX(),
			AddressingMode.IndirectY => IndirectY(),
			AddressingMode.Indirect => mem_read_u16(_programCounter),
			_ => throw new Exception("This addressing mode does not use operands.")
		};
	}

	private ushort IndirectY()
	{
		var a = mem_read(_programCounter);
		var low = mem_read(a);
		var high = mem_read((byte)(a + 1));
		var ptr = (ushort)((high << 8) | low);
		return (ushort)(ptr + _registerY);
	}

	private ushort IndirectX()
	{
		var a = mem_read(_programCounter);
		var ptr = (byte)(a + _registerX);
		var low = mem_read(ptr);
		var high = mem_read((byte)(ptr + 1));
		return (ushort)((high << 8) | low);
	}

	private byte mem_read(ushort address)
	{
		return _memory[address];
	}

	private void mem_write(ushort address, byte value)
	{
		_memory[address] = value;
	}

	private ushort mem_read_u16(ushort address)
	{
		ushort low = mem_read(address);
		ushort high = mem_read((ushort)(address + 1));
		return (ushort)((high << 8) | low);
	}

	private void mem_write_u16(ushort address, ushort data)
	{
		var low = (byte)(data & 0xff);
		var high = (byte)((data >> 8) & 0xff);
		mem_write(address, low);
		mem_write((ushort)(address + 1), high);
	}

	/// <summary>
	///     Used to load and run a program.
	/// </summary>
	/// <param name="program">bytes of program.</param>
	public void LoadAndRun(byte[] program)
	{
		Load(program);
		Reset();
		Run();
	}

	private void Load(byte[] program)
	{
		program.CopyTo(_memory, 0x8000);
		mem_write_u16(0x8000, 0xFFFC);
	}

	private void Reset()
	{
		_programCounter = mem_read(0xFFFC);
		_stackPointer = 0;
		_status = 0;
		_registerA = 0;
		_registerX = 0;
		_registerY = 0;
	}

	public void Run()
	{
		while (true)
		{
			var opcode = CpuOpcodes.OpcodesDict[mem_read(_programCounter)];
			switch (opcode.Mnemonic)
			{
				case BRK:
					return;
				case LDA:
					LoadAccumulator(opcode.AddressingMode);
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
					AddWithCarry(opcode.AddressingMode);
					break;
				case INC:
					IncrementMemory(opcode.AddressingMode);
					break;
				case JMP:
					Jump(opcode.AddressingMode);
					break;
				case NOP:
					break;

				default:
					throw new Exception("Todo or invalid.");
					break;
			}

			_programCounter += opcode.Length;
		}
	}


	private void UpdateZeroFlag(byte value)
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
			_status |= 0b0000_0001;
		else
			_status &= 0b1111_1110;
	}

	private void ClearFlag(CpuFlags flag)
	{
		_status &= (byte)~(byte)flag;
	}

	private void SetFlag(CpuFlags flag)
	{
		_status |= (byte)flag;
	}

	private bool IsFlagSet(CpuFlags flag)
	{
		return (_status & (byte)flag) != 0;
	}

	#region Opcode_Implementations

	public void LoadAccumulator(AddressingMode addressingMode)
	{
		var address = get_operand_address(addressingMode);
		var value = mem_read(address);
		_registerA = value;
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
	}

	public void TransferAccumulatorToX()
	{
		_registerX = _registerA;
		UpdateZeroFlag(_registerX);
		UpdateStatusNegativeFlag(_registerX);
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
		_registerY = _registerA;
		UpdateZeroFlag(_registerY);
		UpdateStatusNegativeFlag(_registerY);
	}

	public void IncrementX()
	{
		_registerY = (byte)(_registerX + 1);
		UpdateStatusNegativeFlag(_registerX);
		UpdateZeroFlag(_registerX);
	}

	public void IncrementY()
	{
		_registerY = (byte)(_registerY + 1);
		UpdateStatusNegativeFlag(_registerY);
		UpdateZeroFlag(_registerY);
	}

	public void AddWithCarry(AddressingMode mode)
	{
		var value = mem_read(get_operand_address(mode));
		var carry = _status & 0b0000_0001;
		var a = _registerA + value + carry;
		_registerA = (byte)a;
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
		if ((a & 0b11110000) != 0) UpdateStatusCarryFlag(true);
	}

	public void AndWithAccumulator(AddressingMode mode)
	{
		var value = mem_read(get_operand_address(mode));
		_registerA &= value;
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
	}

	public void ArithmeticShiftLeft(AddressingMode mode)
	{
		byte value;
		value = mode == AddressingMode.Accumulator ? _registerA : mem_read(get_operand_address(mode));
		// Shift the value left. Bit 0 gets set to 0.
		var shiftedValue = (byte)(value << 1);

		// Update memory with shifted value
		if (mode == AddressingMode.Accumulator)
			_registerA = shiftedValue;
		else
			mem_write(get_operand_address(mode), shiftedValue);

		// Update status flags
		UpdateZeroFlag(shiftedValue);
		UpdateStatusNegativeFlag(shiftedValue);

		// Set the Carry Flag based on bit 7 of the original value
		if ((value & 0b1000_0000) != 0)
			_status |= (byte)CpuFlags.Carry;
		else
			_status &= (byte)~CpuFlags.Carry;
	}

	public void BranchifCarryClear(AddressingMode mode)
	{
		//if the carry flag is clear

		//the next value is treated as a signed byte.
		var unsigned = mem_read(get_operand_address(AddressingMode.Immediate));
		var signed = (sbyte)unsigned;
		//add the signed byte to the program counter's value.
		var res = _programCounter + signed;
		_programCounter = (byte)res;
	}

	private void Jump(AddressingMode addressingMode)
	{
		switch (addressingMode)
		{
			case AddressingMode.Absolute:
				//Jumps to the address specified in the next byte.
				_programCounter = mem_read(get_operand_address(addressingMode));
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
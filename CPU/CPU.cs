using static NestedSharp.InstructionType;

namespace NestedSharp;

/// <summary>
///     Emulation of the 6502 CPU inside a NES Console.
/// </summary>
public partial class Cpu
{
	private const ushort StackStart = 0x0100; //from 0x0100 to 0x01FF, growing downwards
	private const byte StackReset = 0xFF;
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
		_stackPointer = StackReset;
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


	public byte StackPop()
	{
		var address = (ushort)(StackStart + _stackPointer);
		var value = mem_read(address);
		_stackPointer++;
		return value;
	}

	public void StackPush(byte value)
	{
		var address = (ushort)(StackStart + _stackPointer);
		mem_write(address, value);
		_stackPointer--;
	}

	#region Memory

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

	#endregion
}
using static NestedSharp.AddressingMode;
using static NestedSharp.InstructionType;

namespace NestedSharp;

public static class CpuOpcodes
{
	public static Dictionary<byte, Opcode> OpcodesDict = new()
	{
		{ 0x00, new Opcode(0x00, BRK, 1, 7, Implied) },
		{ 0xaa, new Opcode(0xaa, TAX, 1, 2, Implied) },
		{ 0xe8, new Opcode(0xe8, INX, 1, 2, Implied) },
		{ 0xc8, new Opcode(0xc8, INY, 1, 2, Implied) },
		{ 0xa9, new Opcode(0xa9, LDA, 2, 2, Immediate) },
		{ 0xa5, new Opcode(0xa5, LDA, 2, 3, ZeroPage) },
		{ 0xb5, new Opcode(0xb5, LDA, 2, 4, ZeroPageX) },
		{ 0xad, new Opcode(0xad, LDA, 3, 4, Absolute) },
		{ 0xbd, new Opcode(0xbd, LDA, 3, 4, AbsoluteX) },
		{ 0xb9, new Opcode(0xb9, LDA, 3, 4, AbsoluteY) },
		{ 0xa1, new Opcode(0xa1, LDA, 2, 6, IndirectX) },
		{ 0xb1, new Opcode(0xb1, LDA, 2, 5, IndirectY) },
		{ 0x85, new Opcode(0x85, STA, 2, 3, ZeroPage) },
		{ 0x95, new Opcode(0x95, STA, 2, 4, ZeroPageX) },
		{ 0x8d, new Opcode(0x8d, STA, 3, 4, Absolute) },
		{ 0x9d, new Opcode(0x9d, STA, 3, 5, AbsoluteX) },
		{ 0x99, new Opcode(0x99, STA, 3, 5, AbsoluteY) },
		{ 0x81, new Opcode(0x81, STA, 2, 6, IndirectX) },
		{ 0x91, new Opcode(0x91, STA, 2, 6, IndirectY) }
	};
}
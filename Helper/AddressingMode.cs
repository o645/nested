namespace NestedSharp;

public enum AddressingMode
{
	Accumulator,
	Immediate,
	ZeroPage,
	ZeroPageX,
	ZeroPageY,
	Absolute,
	AbsoluteX,
	AbsoluteY,
	IndirectX,
	IndirectY,
	Implied,
	Relative, //Used for branching
	Indirect // only used for Jump.
}
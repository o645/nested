namespace NestedSharp;

public class Opcode
{
	public readonly AddressingMode AddressingMode;
	public readonly byte Code;
	public readonly byte Cycles;
	public readonly byte Length;
	public readonly InstructionType Mnemonic;

	public Opcode(byte code, InstructionType mnemonic, byte length, byte cycles, AddressingMode addressingMode)
	{
		Code = code;
		Mnemonic = mnemonic;
		Length = length;
		Cycles = cycles;
		AddressingMode = addressingMode;
	}

	public Opcode(byte code, string mnemonic, byte length, byte cycles, AddressingMode addressingMode)
	{
		Code = code;
		if (!Enum.TryParse<InstructionType>(mnemonic, out var result))
			throw new Exception("Invalid mnemonic");
		Mnemonic = result;
		Length = length;
		Cycles = cycles;
		AddressingMode = addressingMode;
	}
}
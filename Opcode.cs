namespace NestedSharp;

public class Opcode
{
    public AddressingMode addressingMode;
    public byte code;
    public byte cycles;
    public byte length;
    public InstructionType mnemonic;

    public Opcode(byte code, InstructionType mnemonic, byte length, byte cycles, AddressingMode addressingMode)
    {
        this.code = code;
        this.mnemonic = mnemonic;
        this.length = length;
        this.cycles = cycles;
        this.addressingMode = addressingMode;
    }

    public Opcode(byte code, string mnemonic, byte length, byte cycles, AddressingMode addressingMode)
    {
        this.code = code;
        if (!Enum.TryParse<InstructionType>(mnemonic, out var result))
            throw new Exception("Invalid mnemonic");
        this.mnemonic = result;
        this.length = length;
        this.cycles = cycles;
        this.addressingMode = addressingMode;
    }
}
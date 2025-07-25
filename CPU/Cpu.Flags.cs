namespace NestedSharp;

public partial class Cpu
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
	
	private void ClearFlag(CpuFlags flag) => ClearFlag(ref _status, flag);

	private void ClearFlag(ref byte status, CpuFlags flag)
	{
		status &= (byte)~(byte)flag;
	}

	public void SetFlag(CpuFlags flag) => SetFlag(ref _status, flag);
	private void SetFlag(ref byte status, CpuFlags flag)
	{
		status |= (byte)flag;
	}

	private bool IsFlagSet(CpuFlags flag) => IsFlagSet(_status, flag);
	private bool IsFlagSet(byte status, CpuFlags flag)
	{
		return (status & (byte)flag) != 0;
	}
}
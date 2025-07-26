namespace NestedSharp;

public partial class Cpu
{
	public void BranchifCarryClear(AddressingMode mode)
	{
		//if the carry flag is clear

		//the next value is treated as a signed byte.
		var unsigned = mem_read(AddressingMode.Immediate);
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
				_programCounter = mem_read(addressingMode);
				break;
			case AddressingMode.Indirect:

				break;
		}
	}

	#region Transfer_Instructions

	public void LoadAccumulator(AddressingMode addressingMode)
	{
		var address = get_operand_address(addressingMode);
		var value = mem_read(address);
		_registerA = value;
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
	}

	public void LoadXRegister(AddressingMode addressingMode)
	{
		var value = mem_read(addressingMode);
		_registerX = value;
		UpdateZeroFlag(_registerX);
		UpdateStatusNegativeFlag(_registerX);
	}

	public void LoadYRegister(AddressingMode addressingMode)
	{
		var value = mem_read(addressingMode);
		_registerY = value;
		UpdateZeroFlag(_registerY);
		UpdateStatusNegativeFlag(_registerY);
	}

	public void StoreAccumulator(AddressingMode addressingMode)
	{
		mem_write(addressingMode, _registerA);
	}

	public void StoreXRegister(AddressingMode addressingMode)
	{
		mem_write(addressingMode, _registerX);
	}

	public void StoreYRegister(AddressingMode addressingMode)
	{
		mem_write(addressingMode, _registerY);
	}

	public void TransferAccumulatorToX()
	{
		_registerX = _registerA;
		UpdateZeroFlag(_registerX);
		UpdateStatusNegativeFlag(_registerX);
	}

	public void TransferAccumulatorToY()
	{
		_registerY = _registerA;
		UpdateZeroFlag(_registerY);
		UpdateStatusNegativeFlag(_registerY);
	}

	public void TransferStackPointerToX()
	{
		_registerX = _stackPointer;
		UpdateZeroFlag(_registerX);
		UpdateStatusNegativeFlag(_registerX);
	}

	public void TransferXtoAccumulator()
	{
		_registerA = _registerX;
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
	}

	public void TransferXtoStackPointer()
	{
		_stackPointer = _registerX;
	}

	public void TransferYtoAccumulator()
	{
		_registerA = _registerY;
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
	}

	#endregion

	#region Stack_Instructions

	public void PushAccumulator()
	{
		StackPush(_registerA);
	}

	public void PushProcessorStatus()
	{
		var flags = _status;
		SetFlag(ref flags, CpuFlags.BreakCommand);
		SetFlag(ref flags, CpuFlags.Unused);
		StackPush(flags);
	}

	public void PullAccumulator()
	{
		_registerA = StackPop();
	}

	public void PullProcessorStatus()
	{
		var flags = StackPop();
		ClearFlag(ref flags, CpuFlags.BreakCommand);
		ClearFlag(ref flags, CpuFlags.Unused);
	}

	#endregion

	#region Decrement_Increment_Instructions

	public void IncrementMemory(AddressingMode addressingMode)
	{
		var value = mem_read(addressingMode);
		value++;
		mem_write(addressingMode, value);
		UpdateStatusNegativeFlag(value);
		UpdateZeroFlag(value);
	}

	public void DecrementMemory(AddressingMode addressingMode)
	{
		var value = mem_read(addressingMode);
		value--;
		mem_write(addressingMode, value);
		UpdateStatusNegativeFlag(value);
		UpdateZeroFlag(value);
	}

	public void IncrementX()
	{
		_registerY = (byte)(_registerX + 1);
		UpdateStatusNegativeFlag(_registerX);
		UpdateZeroFlag(_registerX);
	}

	public void DecrementX()
	{
		_registerX = (byte)(_registerX - 1);
		UpdateStatusNegativeFlag(_registerX);
		UpdateZeroFlag(_registerX);
	}

	public void IncrementY()
	{
		_registerY = (byte)(_registerY + 1);
		UpdateStatusNegativeFlag(_registerY);
		UpdateZeroFlag(_registerY);
	}

	public void DecrementY()
	{
		_registerY = (byte)(_registerY - 1);
		UpdateStatusNegativeFlag(_registerY);
		UpdateZeroFlag(_registerY);
	}

	#endregion

	#region Arithmetic_Instructions

	public void AddWithCarry(AddressingMode mode)
	{
		var value = mem_read(mode);
		var carry = IsFlagSet(CpuFlags.Carry) ? 1 : 0;
		_registerA = (byte)(_registerA + value + carry);
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
		if (_registerA + value + carry > Byte.MaxValue) UpdateStatusCarryFlag(true);
	}

	public void SubtractWithCarry(AddressingMode mode)
	{
		var value = mem_read(mode);
		var carry = IsFlagSet(CpuFlags.Carry) ? 1 : 0;
		_registerA = (byte)(_registerA - value - carry);
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
		if (_registerA + value + carry > Byte.MaxValue) UpdateStatusCarryFlag(true);
	}

	#endregion

	#region Logical_Instructions

	public void AndWithAccumulator(AddressingMode mode)
	{
		_registerA &= mem_read(mode);
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
	}

	public void ExclusiveORWithAccumulator(AddressingMode mode)
	{
		_registerA ^= mem_read(mode);
		UpdateStatusNegativeFlag(_registerA);
		UpdateZeroFlag(_registerA);
	}

	public void ORWithAccumulator(AddressingMode mode)
	{
		_registerA |= mem_read(mode);
		UpdateStatusNegativeFlag(_registerA);
		UpdateZeroFlag(_registerA);
	}

	#endregion

	#region Shift & Rotate Instructions

	public void ArithmeticShiftLeft(AddressingMode mode)
	{
		byte value = mem_read(mode);
		// Shift the value left. Bit 0 gets set to 0.
		var shiftedValue = (byte)(value << 1);

		// Update memory with shifted value
		mem_write(mode, shiftedValue);

		// Update status flags
		UpdateZeroFlag(shiftedValue);
		UpdateStatusNegativeFlag(shiftedValue);
		SetCarryFlagLeftShift(value);
	}

	public void LogicalShiftRight(AddressingMode mode)
	{
		byte value = mem_read(mode);
		byte shifted = (byte)(value >> 1);
		mem_write(mode, shifted);
		UpdateStatusNegativeFlag(shifted);
		UpdateZeroFlag(shifted);
		SetCarryFlagRightShift(value);
	}

	public void RotateLeft(AddressingMode mode)
	{
		byte value = mem_read(mode);
		byte carry = (byte)(IsFlagSet(CpuFlags.Carry) ? 0b0000_0001 : 0);
		byte rotated = (byte)(value << 1);
		rotated |= carry;
		mem_write(mode, rotated);
		UpdateStatusNegativeFlag(rotated);
		UpdateZeroFlag(rotated);
		SetCarryFlagLeftShift(value);
	}

	public void RotateRight(AddressingMode mode)
	{
		byte value = mem_read(mode);
		byte carry = (byte)(IsFlagSet(CpuFlags.Carry) ? 0b1000_0000 : 0);
		byte rotated = (byte)(1 >> value);
		rotated |= carry;
		mem_write(mode, rotated);
		UpdateStatusNegativeFlag(rotated);
		UpdateZeroFlag(rotated);
		SetCarryFlagRightShift(value);
	}

	#endregion
}
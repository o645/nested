namespace NestedSharp;

public partial class Cpu
{




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
		var address = get_operand_address(addressingMode);
		var value = mem_read(address);
		_registerX = value;
		UpdateZeroFlag(_registerX);
		UpdateStatusNegativeFlag(_registerX);
	}

	public void LoadYRegister(AddressingMode addressingMode)
	{
		var address = get_operand_address(addressingMode);
		var value = mem_read(address);
		_registerY = value;
		UpdateZeroFlag(_registerY);
		UpdateStatusNegativeFlag(_registerY);
	}

	public void StoreAccumulator(AddressingMode addressingMode)
	{
		var address = get_operand_address(addressingMode);
		mem_write(address, _registerA);
	}

	public void StoreXRegister(AddressingMode addressingMode)
	{
		var address = get_operand_address(addressingMode);
		mem_write(address, _registerX);
	}

	public void StoreYRegister(AddressingMode addressingMode)
	{
		var address = get_operand_address(addressingMode);
		mem_write(address, _registerY);
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
		ClearFlag(ref flags,CpuFlags.BreakCommand);
		ClearFlag(ref flags,CpuFlags.Unused);
	}

	#endregion
	
	#region Decrement_Increment_Instructions

	public void IncrementMemory(AddressingMode addressingMode)
	{
		var value = mem_read(get_operand_address(addressingMode));
		value++;
		mem_write(get_operand_address(addressingMode), value);
		UpdateStatusNegativeFlag(value);
		UpdateZeroFlag(value);
	}

	public void DecrementMemory(AddressingMode addressingMode)
	{
		var value = mem_read(get_operand_address(addressingMode));
		value--;
		mem_write(get_operand_address(addressingMode), value);
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
		var value = mem_read(get_operand_address(mode));
		var carry = IsFlagSet(CpuFlags.Carry) ? 1 : 0;
		_registerA = (byte)(_registerA + value + carry);
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
		if (_registerA + value + carry > Byte.MaxValue) UpdateStatusCarryFlag(true);
	}

	public void SubtractWithCarry(AddressingMode mode)
	{
		var value = mem_read(get_operand_address(mode));
		var carry = IsFlagSet(CpuFlags.Carry) ? 1 : 0;
		_registerA  = (byte)(_registerA - value - carry);
		UpdateZeroFlag(_registerA);
		UpdateStatusNegativeFlag(_registerA);
		if (_registerA + value + carry > Byte.MaxValue) UpdateStatusCarryFlag(true);
	}
	#endregion
}
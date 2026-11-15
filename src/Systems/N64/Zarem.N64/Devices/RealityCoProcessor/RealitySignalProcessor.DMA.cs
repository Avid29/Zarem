// Avishai Dernis 2026

using System.Buffers;
using Zarem.Emulator.Extensions;
using Zarem.N64.Devices.RealityCoProcessor.Enums;

namespace Zarem.N64.Devices.RealityCoProcessor;

public partial class RealitySignalProcessor
{

    private void CommandRegisterWrite(RspRegister register, uint value)
    {
        switch (register)
        {
            // Handle status register side effects
            case RspRegister.Status:
                HandleStatusRegisterWrite(value);
                break;
            case RspRegister.DmaReadLength:
                ExecuteDmaTransfer(value, false);
                break;
            case RspRegister.DmaWriteLength:
                ExecuteDmaTransfer(value, true);
                break;
            case RspRegister.Semaphore:
                // TODO: Handle semaphore lock/clear
                break;
        }
    }

    private void ExecuteDmaTransfer(uint command, bool writeToDram)
    {
        // N64 DMA length registers encode length and count/skip parameters:
        uint transferLength = (command & 0xFFF) + 1;
        uint count = ((command >> 12) & 0xFF) + 1;
        uint skip = (command >> 20) & 0xFFF;

        uint rspMemAddr = _regs.Read<uint>((int)RspRegister.DmaMemoryAddress) & 0x1FFF;
        uint dramAddr = _regs.Read<uint>((int)RspRegister.DmaDramAddress) & 0x00FFFFFF;

        // Determine target memory store once (IMEM vs DMEM) based on bit 12 of the RSP address
        bool isIMem = (rspMemAddr & 0x1000) != 0;
        var rspMemory = isIMem ? _iMem : _dMem;
        uint rspMemOffset = rspMemAddr & 0x0FFF;

        // Allocate copy buffer
        byte[]? pooledArray = null;
        var span = transferLength <= 1024 ?
            stackalloc byte[(int)transferLength] :
            (pooledArray = ArrayPool<byte>.Shared.Rent((int)transferLength));

        // Perform block transfer between DMEM/IMEM and main memory (RDRAM)
        for (uint i = 0; i < count; i++)
        {
            uint currentRspMemAddr = rspMemOffset + (i * transferLength) + (i * skip);
            uint currentDramAddr = dramAddr + (i * transferLength);

            // TODO - Optimization: Replace bus calls with direct use of RDRAM instance

            if (writeToDram)
            {
                // Write: RSP Memory -> RDRAM
                rspMemory.Read(currentRspMemAddr, span);
                _bus.Write(currentDramAddr, span);
            }
            else
            {
                // Read: RDRAM -> RSP Memory
                _bus.Read(currentDramAddr, span);
                rspMemory.Write(currentRspMemAddr, span);
            }

            // TODO: Write optimized copy for continuous blocks (skip is 0)
        }

        // Free temporary array
        if (pooledArray is not null)
        {
            ArrayPool<byte>.Shared.Return(pooledArray);
        }

        // Clear DMA busy bit upon completion
        _regs.Write((int)RspRegister.DmaBusy, 0);
    }

    private void HandleStatusRegisterWrite(uint command)
    {
        // Fetch status value
        uint currentStatus = _regs.Read<uint>((int)RspRegister.Status);

        // Modify status value
        if ((command & 0x0001) != 0) currentStatus &= ~0x0001u; // Clear Halt
        if ((command & 0x0002) != 0) currentStatus |= 0x0001u;  // Set Halt
        if ((command & 0x0004) != 0) currentStatus &= ~0x0002u; // Clear Broke
        if ((command & 0x0008) != 0) currentStatus |= 0x0004u;  // Set Intr on Break
        if ((command & 0x0010) != 0) currentStatus &= ~0x0004u; // Clear Intr on Break

        // Writeback new status value
        _regs.Write((int)RspRegister.Status, currentStatus);
    }
}

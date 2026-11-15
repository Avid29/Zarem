// Avishai Dernis 2026

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices.Marshalling;
using Zarem.Emulator.Extensions;

namespace Zarem.N64.Devices.RealityCoProcessor;

public partial class RealitySignalProcessor
{
    private enum RspRegister
    {
        /// <remarks>
        /// SP_MEM_ADDR_REG
        /// </remarks>
        DmaMemoryAddress = 0x00,

        /// <remarks>
        /// SP_DRAM_ADDR_REG
        /// </remarks>
        DmaDramAddress = 0x04,

        /// <remarks>
        /// SP_RD_LEN_REG
        /// </remarks>
        DmaReadLength = 0x8,

        /// <remarks>
        /// SP_WR_LEN_REG
        /// </remarks>
        DmaWriteLength = 0xC,

        /// <remarks>
        /// SP_STATUS_REG
        /// </remarks>
        Status = 0x10,

        /// <remarks>
        /// SP_DMA_FULL_REG
        /// </remarks>
        DmaFull = 0x14,

        /// <remarks>
        /// SP_DMA_BUSY_REG
        /// </remarks>
        DmaBusy = 0x18,

        /// <remarks>
        /// SP_SEMAPHORE_REG
        /// </remarks>
        Semaphore = 0x1C,
    }

    private void HandleRegisterSideEffects(RspRegister register, uint value)
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

        byte[]? pooledArray = null;
        var span = transferLength <= 1024 ?
            stackalloc byte[(int)transferLength] :
            (pooledArray = ArrayPool<byte>.Shared.Rent((int)transferLength));

        // Perform block transfer between DMEM/IMEM and main memory (RDRAM)
        for (uint i = 0; i < count; i++)
        {
            uint currentRspMemAddr = rspMemOffset + (i * transferLength) + (i * skip);
            uint currentDramAddr = dramAddr + (i * transferLength);

            // TODO: Optimize bus calls to instead use RDRAM

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

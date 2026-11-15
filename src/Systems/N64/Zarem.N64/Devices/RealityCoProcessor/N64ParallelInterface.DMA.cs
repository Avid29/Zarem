// Avishai Dernis 2026

using System.Buffers;
using Zarem.Emulator.Extensions;
using Zarem.N64.Devices.RealityCoProcessor.Enums;

namespace Zarem.N64.Devices.RealityCoProcessor;

public partial class N64ParallelInterface 
{
    private void ExcecuteDmaTransfer(uint length, bool writeToDram)
    {
        // Read current 24-bit RDRAM address and 32-bit cartridge physical address
        uint dramAddr = _regs.Read<uint>((int)PiRegister.DramAddress) & 0x00FF_FFFF;
        uint cartAddr = _regs.Read<uint>((int)PiRegister.CartridgeAddress);

        // N64 PI DMA length registers store (transfer_size - 1)
        uint byteCount = length + 1;

        // Mark PI controller as busy during transfer (bit 0 = DMA busy)
        uint status = _regs.Read<uint>((int)PiRegister.Status);
        status |= 0x01;
        _regs.Write((int)PiRegister.Status, status);

        // Allocate copy buffer
        byte[]? pooledArray = null;
        var span = byteCount <= 1024 ?
            stackalloc byte[(int)byteCount] :
            (pooledArray = ArrayPool<byte>.Shared.Rent((int)byteCount));

        // TODO - Optimization: Replace bus calls with direct use of Cartridge and RDRAM instance

        if (writeToDram)
        {
            // Cartridge ROM -> RDRAM
            _bus.Read(cartAddr, span);
            _bus.Write(dramAddr, span);
        }
        else
        {
            // RDRAM -> Cartridge ROM (Flash/SRAM/etc.)
            _bus.Read(dramAddr, span);
            _bus.Write(cartAddr, span);
        }

        // Free temporary array
        if (pooledArray is not null)
        {
            ArrayPool<byte>.Shared.Return(pooledArray);
        }

        // Update address registers to point to the end of the transfer
        _regs.Write((int)PiRegister.DramAddress, (dramAddr + byteCount) & 0x00FF_FFFF);
        _regs.Write((int)PiRegister.CartridgeAddress, cartAddr + byteCount);

        // Clear DMA busy bit upon completion (and optionally assert PI interrupt flag if your system models interrupts)
        status = _regs.Read<uint>((int)PiRegister.Status);
        status &= ~0x01U; // Clear DMA busy
        status |= 0x04U;   // Set PI interrupt flag (if modeling controller interrupts)
        _regs.Write((int)PiRegister.Status, status);
    }

    private void HandleStatusWrite(uint value)
    {
        // Fetch status value
        uint currentStatus = _regs.Read<uint>((int)PiRegister.Status);

        // Modify status value
        if ((value & 0x1) != 0) currentStatus &= ~0x04U; // Clear PI interrupt
        if ((value & 0x2) != 0) currentStatus = 0;      // Reset PI controller

        // Writeback new status value
        _regs.Write((int)PiRegister.Status, currentStatus);
    }
}

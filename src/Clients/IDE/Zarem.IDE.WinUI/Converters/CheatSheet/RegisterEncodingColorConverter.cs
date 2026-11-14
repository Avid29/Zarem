// Avishai Dernis 2025

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.IDE.Converters;

public partial class RegisterEncodingColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not MipsGpRegister reg)
            return null;

        return reg switch
        {
            MipsGpRegister.Zero or MipsGpRegister.AssemblerTemporary => OtherBrush,
            MipsGpRegister.ReturnValue0 or MipsGpRegister.ReturnValue1 => ReturnValueBrush,
            >= MipsGpRegister.Argument0 and <= MipsGpRegister.Argument3 => ArgBrush,
            (>= MipsGpRegister.Temporary0 and <= MipsGpRegister.Temporary7)
            or MipsGpRegister.Temporary8 or MipsGpRegister.Temporary9 => TempBrush,
            >= MipsGpRegister.Saved0 and <= MipsGpRegister.Saved7 => SavedBrush,
            MipsGpRegister.Kernel0 or MipsGpRegister.Kernel1 => KernelBrush,
            MipsGpRegister.GlobalPointer or MipsGpRegister.StackPointer or MipsGpRegister.FramePointer => EnvironmentBrush,
            MipsGpRegister.ReturnAddress => ReturnAddressBrush,
            _ => null
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    public Brush? ReturnValueBrush { get; set; }

    public Brush? ArgBrush { get; set; }
    
    public Brush? TempBrush { get; set; }
    
    public Brush? SavedBrush { get; set; }
    
    public Brush? KernelBrush { get; set; }
    
    public Brush? EnvironmentBrush { get; set; }
    
    public Brush? ReturnAddressBrush { get; set; }

    public Brush? OtherBrush { get; set; }
}

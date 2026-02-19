// Avishai Dernis 2025

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.WinUI.Converters;

public partial class RegisterEncodingColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not GPRegister reg)
            return null;

        return reg switch
        {
            GPRegister.Zero or GPRegister.AssemblerTemporary => OtherBrush,
            GPRegister.ReturnValue0 or GPRegister.ReturnValue1 => ReturnValueBrush,
            >= GPRegister.Argument0 and <= GPRegister.Argument3 => ArgBrush,
            (>= GPRegister.Temporary0 and <= GPRegister.Temporary7)
            or GPRegister.Temporary8 or GPRegister.Temporary9 => TempBrush,
            >= GPRegister.Saved0 and <= GPRegister.Saved7 => SavedBrush,
            GPRegister.Kernel0 or GPRegister.Kernel1 => KernelBrush,
            GPRegister.GlobalPointer or GPRegister.StackPointer or GPRegister.FramePointer => EnvironmentBrush,
            GPRegister.ReturnAddress => ReturnAddressBrush,
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

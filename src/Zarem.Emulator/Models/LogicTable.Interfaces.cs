// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.Models;

public partial class LogicTable
{
    /// <summary>
    /// An interface for shift logic operations.
    /// </summary>
    public interface IShiftLogic<T>
        where T : unmanaged, IBinaryInteger<T>
    {
        /// <summary>
        /// Executes the shift logic operation.
        /// </summary>
        static abstract T Execute(T rt, int sa);
    }

    /// <summary>
    /// An interface for ALU logic operations.
    /// </summary>
    public interface IAluLogic<T>
        where T : unmanaged, INumber<T>
    {
        /// <summary>
        /// Executes the ALU logic operation.
        /// </summary>
        static abstract T Compute(T rs, T rt);
    }

    /// <summary>
    /// An interface for floating-point ALU logic operations.
    /// </summary>
    public interface IFAluLogic<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        /// <summary>
        /// Executes the ALU logic operation.
        /// </summary>
        static abstract T Compute(T fs);
    }

    /// <summary>
    /// An interface for a rounding operations.
    /// </summary>
    public interface IRoundLogic<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        /// <summary>
        /// Executes the round operation.
        /// </summary>
        static abstract T Compute(T rs);
    }

    /// <summary>
    /// An interface for ALU logic operations with an overflow check.
    /// </summary>
    public interface ICheckedAluLogic<T> : IAluLogic<T>
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
    {
        /// <summary>
        /// Checks if an overflow occured.
        /// </summary>
        static abstract bool Overflow(T a, T b, T r);
    }

    /// <summary>
    /// An interface for multiply logic.
    /// </summary>
    public interface IMultLogic<T, TL>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TL : unmanaged, IBinaryInteger<TL>, IUnsignedNumber<TL>
    {
        /// <summary>
        /// Executes the multiply logic operation.
        /// </summary>
        static abstract TL Compute(T rs, T rt);
    }

    /// <summary>
    /// An interface for multiply and add logic operations.
    /// </summary>
    public interface IMultAddLogic<T, TL>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        where TL : unmanaged, IBinaryInteger<TL>, IUnsignedNumber<TL>
    {
        /// <summary>
        /// Executes the multiply and add logic operation.
        /// </summary>
        static abstract TL Compute(T rs, T rt, TL @base);
    }

    /// <summary>
    /// An interface for divide logic operations.
    /// </summary>
    public interface IDivLogic<T>
        where T : unmanaged, INumber<T>
    {
        /// <summary>
        /// Executes the divisor component divide logic operation.
        /// </summary>
        static abstract T Divisor(T rs, T rt);

        /// <summary>
        /// Executes the remainder component divide logic operation.
        /// </summary>
        static abstract T Remainder(T rs, T rt);
    }

    /// <summary>
    /// An interface for a conditional logic operation.
    /// </summary>
    public interface ICondLogic<T>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    {
        /// <summary>
        /// Executes the conditional logic operation.
        /// </summary>
        static abstract bool Check(T rs, T rt);
    }
}

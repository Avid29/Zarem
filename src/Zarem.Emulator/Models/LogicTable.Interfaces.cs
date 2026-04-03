// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.Models;

public partial class LogicTable<T, TSigned>
{
    /// <summary>
    /// An interface for shift logic operations.
    /// </summary>
    public interface IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        /// <summary>
        /// Executes the shift logic operation.
        /// </summary>
        static abstract T2 Execute(T2 rt, int sa);
    }

    /// <summary>
    /// An interface for ALU logic operations.
    /// </summary>
    public interface IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        /// <summary>
        /// Executes the ALU logic operation.
        /// </summary>
        static abstract T2 Compute(T2 rs, T2 rt);
    }

    /// <summary>
    /// An interface for ALU logic operations with an overflow check.
    /// </summary>
    public interface ICheckedAluLogic<T2, TSigned2> : IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TSigned2 : unmanaged, IBinaryInteger<TSigned2>, ISignedNumber<TSigned2>
    {
        /// <summary>
        /// Checks if an overflow occured.
        /// </summary>
        static abstract bool Overflow(TSigned2 a, TSigned2 b, TSigned2 r);
    }

    /// <summary>
    /// An interface for multiply logic.
    /// </summary>
    public interface IMultLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        /// <summary>
        /// Executes the multiply logic operation.
        /// </summary>
        static abstract (T2, T2) Compute(T2 rs, T2 rt);
    }

    /// <summary>
    /// An interface for multiply and add logic operations.
    /// </summary>
    public interface IMultAddLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        /// <summary>
        /// Executes the multiply and add logic operation.
        /// </summary>
        static abstract (T2, T2) Compute(T2 rs, T2 rt, T2 hi, T2 lo);
    }

    /// <summary>
    /// An interface for divide logic operations.
    /// </summary>
    public interface IDivLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        /// <summary>
        /// Executes the divisor component divide logic operation.
        /// </summary>
        static abstract T2 Divisor(T2 rs, T2 rt);

        /// <summary>
        /// Executes the remainder component divide logic operation.
        /// </summary>
        static abstract T2 Remainder(T2 rs, T2 rt);
    }

    /// <summary>
    /// An interface for a conditional logic operation.
    /// </summary>
    public interface ICondLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        /// <summary>
        /// Executes the conditional logic operation.
        /// </summary>
        static abstract bool Check(T2 rs, T2 rt);
    }
}

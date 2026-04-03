// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Models;

/// <summary>
/// A base class for an instruction service table.
/// </summary>
public partial class LogicTable<T, TSigned>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
{
    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for an AND logic operation.
    /// </summary>
    public struct AndLogic : IAluLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs & rt;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for an OR logic operation.
    /// </summary>
    public struct OrLogic : IAluLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs | rt;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for an XOR logic operation.
    /// </summary>
    public struct XorLogic : IAluLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs ^ rt;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for a NOR logic operation.
    /// </summary>
    public struct NorLogic : IAluLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => ~(rs | rt);
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a signed greater than or equal to logic operation.
    /// </summary>
    public struct XgeLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateTruncating(rs) >= TSigned.CreateTruncating(rt);
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for an unsigned greater than or equal to logic operation.
    /// </summary>
    public struct XgeuLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs >= rt;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a signed less than to logic operation.
    /// </summary>
    public struct XltLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateTruncating(rs) < TSigned.CreateTruncating(rt);
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for an unsigned less than to logic operation.
    /// </summary>
    public struct XltuLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs < rt;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for an equal to logic operation.
    /// </summary>
    public struct XeqLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs == rt;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a not equal to logic operation.
    /// </summary>
    public struct XneLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs != rt;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a less than or equal to zero logic operation.
    /// </summary>
    public struct XlezLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateTruncating(rs) <= TSigned.Zero;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a less than zero logic operation.
    /// </summary>
    public struct XltzLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateTruncating(rs) < TSigned.Zero;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a greater than or equal to zero logic operation.
    /// </summary>
    public struct XgezLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateTruncating(rs) >= TSigned.Zero;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a greater than zero logic operation.
    /// </summary>
    public struct XgtzLogic : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateTruncating(rs) > TSigned.Zero;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for an equal to zero logic operation.
    /// </summary>
    public struct Xeqz : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rt == T.Zero;
    }

    /// <summary>
    /// An <see cref="ICondLogic{T}"/> implementation for a not equal to zero logic operation.
    /// </summary>
    public struct Xnez : ICondLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rt != T.Zero;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for a signed set less than logic operation.
    /// </summary>
    public struct SltLogic : IAluLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => TSigned.CreateTruncating(rs) < TSigned.CreateTruncating(rt) ? T.One : T.Zero;
    }

    /// <summary>
    /// An <see cref="IAluLogic{T}"/> implementation for an unsigned set less than logic operation.
    /// </summary>
    public struct SltuLogic : IAluLogic<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs < rt ? T.One : T.Zero;
    }
}

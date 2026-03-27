// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Models.Enum;

namespace Zarem.Emulator.Models;

public partial class InstructionServiceTable<T, TSigned>
{
    private interface IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        static abstract T2 Execute(T2 rt, int sa);
    }

    private interface IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        static abstract T2 Compute(T2 rs, T2 rt);
    }

    private interface ICheckedAluLogic<T2, TSigned2> : IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TSigned2 : unmanaged, IBinaryInteger<TSigned2>, ISignedNumber<TSigned2>
    {
        static abstract bool Overflow(TSigned2 a, TSigned2 b, TSigned2 r);
    }

    private interface IMultLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        static abstract (T2, T2) Compute(T2 rs, T2 rt);
    }

    private interface IMultAddLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        static abstract (T2, T2) Compute(T2 rs, T2 rt, T2 hi, T2 lo);
    }

    private interface IDivLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        static abstract T2 Divisor(T2 rs, T2 rt);

        static abstract T2 Remainder(T2 rs, T2 rt);
    }

    private interface ITrapLogic
    {
        static abstract MipsTrap Trap();
    }

    private interface ICondLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        static abstract bool Check(T2 rs, T2 rt);
    }

    private struct AndLogic : IAluLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs & rt;
    }

    private struct OrLogic : IAluLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs | rt;
    }

    private struct XorLogic : IAluLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs ^ rt;
    }

    private struct NorLogic : IAluLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => ~(rs | rt);
    }

    private struct XgeLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateSaturating(rs) >= TSigned.CreateSaturating(rt);
    }

    private struct XgeuLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs >= rt;
    }

    private struct XltLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateSaturating(rs) < TSigned.CreateSaturating(rt);
    }

    private struct XltuLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs < rt;
    }

    private struct XeqLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs == rt;
    }

    private struct XneLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rs != rt;
    }

    private struct XlezLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateSaturating(rs) <= TSigned.Zero;
    }

    private struct XltzLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateSaturating(rs) < TSigned.Zero;
    }

    private struct XgezLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateSaturating(rs) >= TSigned.Zero;
    }

    private struct XgtzLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => TSigned.CreateSaturating(rs) > TSigned.Zero;
    }

    private struct MovzLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rt == T.Zero;
    }

    private struct MovnLogic : ICondLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(T rs, T rt) => rt != T.Zero;
    }

    private struct SltLogic : IAluLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => TSigned.CreateSaturating(rs) < TSigned.CreateSaturating(rt) ? T.One : T.Zero;
    }

    private struct SltuLogic : IAluLogic<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Compute(T rs, T rt) => rs < rt ? T.One : T.Zero;
    }

    private struct SyscallLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Syscall;
    }

    private struct BreakLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Breakpoint;
    }

    private struct TrapLogic : ITrapLogic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MipsTrap Trap() => MipsTrap.Trap;
    }

}

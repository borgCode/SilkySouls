using System;

namespace SilkySouls.Memory
{
    public static class AsmHelper
    {
        public static int GetRelOffset(IntPtr srcInstrAddr, IntPtr targetAddr, int instrLength = 0)
            => (int)(targetAddr.ToInt64() - (srcInstrAddr.ToInt64() + instrLength));

        public static byte[] GetRelOffsetBytes(IntPtr srcInstrAddr, IntPtr targetAddr, int instrLength = 0)
            => BitConverter.GetBytes(GetRelOffset(srcInstrAddr, targetAddr, instrLength));

        public static int GetRelOffset(long srcInstrAddr, long targetAddr, int instrLength = 0)
            => (int)(targetAddr - (srcInstrAddr + instrLength));

        public static byte[] GetRelOffsetBytes(long srcInstrAddr, long targetAddr, int instrLength = 0)
            => BitConverter.GetBytes(GetRelOffset(srcInstrAddr, targetAddr, instrLength));
    }
}
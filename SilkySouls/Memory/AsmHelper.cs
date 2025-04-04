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

        public static void WriteRelativeOffsets(byte[] bytes, (long baseAddr, long targetAddr, int size, int offset)[] offsets)
        {
            foreach (var (baseAddr, targetAddr, size, offset) in offsets)
            {
                var relativeBytes = GetRelOffsetBytes(baseAddr, targetAddr, size);
                Array.Copy(relativeBytes, 0, bytes, offset, 4);
            }
        }
    }
}
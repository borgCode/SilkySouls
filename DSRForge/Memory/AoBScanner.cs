using System;

namespace DSRForge.Memory
{
    public class AoBScanner
    {
        private readonly MemoryIo _memoryIo;
        public AoBScanner(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }
        
        public IntPtr FindAddressByPattern(Pattern pattern)
        {
            IntPtr patternAddress = PatternScan(pattern.Bytes, pattern.Mask);
            if (patternAddress == IntPtr.Zero)
                return IntPtr.Zero; 
            
            IntPtr instructionAddress = IntPtr.Add(patternAddress, pattern.InstructionOffset);
    
            switch (pattern.RipType)
            {
                case RipType.None:
                    return instructionAddress;
            
                case RipType.Standard:
                    // e.g. 48 8B 05/0D - Standard mov rax/rcx,[rip+offset]
                    int stdOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                    return IntPtr.Add(instructionAddress, stdOffset + 7);
            
                case RipType.Comparison:
                    // e.g. 80 3D - cmp byte ptr [rip+offset],imm
                    int cmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 2));
                    return IntPtr.Add(instructionAddress, cmpOffset + 7);
            
                default:
                    return IntPtr.Zero;
            }
        }
        
        public IntPtr PatternScan(byte[] pattern, string mask)
        {
            const int chunkSize = 4096 * 16; 
            byte[] buffer = new byte[chunkSize];
            
            IntPtr currentAddress = _memoryIo.BaseAddress;
            IntPtr endAddress = IntPtr.Add(currentAddress, 0x3200000);
            
            while (currentAddress.ToInt64() < endAddress.ToInt64())
            {
                int bytesRemaining = (int)(endAddress.ToInt64() - currentAddress.ToInt64());
                int bytesToRead = Math.Min(bytesRemaining, buffer.Length);
                
                if (bytesToRead < pattern.Length)
                    break;
                
                buffer = _memoryIo.ReadBytes(currentAddress, bytesToRead);
                
                for (int i = 0; i <= bytesToRead - pattern.Length; i++)
                {
                    bool found = true;
                    
                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (j < mask.Length && mask[j] == '?')
                            continue;
                            
                        if (buffer[i + j] != pattern[j])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                        return IntPtr.Add(currentAddress, i);
                }
                currentAddress = IntPtr.Add(currentAddress, bytesToRead - pattern.Length + 1);
            }
            return IntPtr.Zero;
        }
    }
}
﻿using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DSRForge.memory;

namespace DSRForge.Memory
{
    public class MemoryIo : IDisposable
    {
        public Process TargetProcess;
        public IntPtr ProcessHandle = IntPtr.Zero;
        public IntPtr BaseAddress = new IntPtr(0x140000000);

        private const int ProcessVmRead = 0x0010;
        private const int ProcessVmWrite = 0x0020;
        private const int ProcessVmOperation = 0x0008;

        private const string ProcessName = "darksoulsremastered";
        private bool _disposed = false;
        
        public void FindAndAttach()
        {

            var processes = Process.GetProcessesByName(ProcessName);

            if (processes.Length > 0 && !processes[0].HasExited)
            {
                Attach(processes[0]);
            }
            else
            {
                throw new Exception("DS1 not running");
            }
        }
        
        private void Attach(Process process)
        {
            if (ProcessHandle == IntPtr.Zero)
            {
                TargetProcess = process;
                ProcessHandle = Kernel32.OpenProcess(
                    ProcessVmRead | ProcessVmWrite | ProcessVmOperation,
                    false,
                    TargetProcess.Id);

                if (ProcessHandle == IntPtr.Zero)
                {
                    throw new Exception("Attach failed");
                }

            }
            else
            {
                //Implement showing messagebox ex.message
                throw new Exception("Already attached");

            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (ProcessHandle != IntPtr.Zero)
                {
                    Kernel32.CloseHandle(ProcessHandle);
                    ProcessHandle = IntPtr.Zero;
                    TargetProcess = null;
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~MemoryIo()
        {
            Dispose();
        }

        public bool ReadTest(IntPtr addr)
        {
            var array = new byte[1];
            var lpNumberOfBytesRead = 1;
            return Kernel32.ReadProcessMemory(ProcessHandle, addr, array, 1, ref lpNumberOfBytesRead) && lpNumberOfBytesRead == 1;
        }

        public void ReadTestFull(IntPtr addr)
        {
            Console.WriteLine($"Testing Address: 0x{addr.ToInt64():X}");

            bool available = ReadTest(addr);
            Console.WriteLine($"Availability: {available}");

            if (!available)
            {
                Console.WriteLine("Memory is not readable at this address.");
                return;
            }

            try
            {
                Console.WriteLine($"Int32: {ReadInt32(addr)}");
                Console.WriteLine($"Int64: {ReadInt64(addr)}");
                Console.WriteLine($"UInt8: {ReadUInt8(addr)}");
                Console.WriteLine($"UInt32: {ReadUInt32(addr)}");
                Console.WriteLine($"UInt64: {ReadUInt64(addr)}");
                Console.WriteLine($"Float: {ReadFloat(addr)}");
                Console.WriteLine($"Double: {ReadDouble(addr)}");
                Console.WriteLine($"String: {ReadString(addr)}");

                byte[] bytes = ReadBytes(addr, 16);
                Console.WriteLine("Bytes: " + BitConverter.ToString(bytes));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading memory: " + ex.Message);
            }
        }

        public uint RunThread(IntPtr address, uint timeout = 0xFFFFFFFF)
        {
            IntPtr thread = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, IntPtr.Zero);
            var ret = Kernel32.WaitForSingleObject(thread, timeout);
            Kernel32.CloseHandle(thread);
            return ret;
        }
        
        public bool RunThreadAndWaitForCompletion(IntPtr address, uint timeout = 0xFFFFFFFF)
        {
            IntPtr thread = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, IntPtr.Zero);
            
            if (thread == IntPtr.Zero)
            {
                return false;
            }
    
            uint waitResult = Kernel32.WaitForSingleObject(thread, timeout);
            Kernel32.CloseHandle(thread);
  
            return waitResult == 0;
        }

        public int ReadInt32(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public long ReadInt64(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public byte ReadUInt8(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 1);
            return bytes[0];
        }

        public uint ReadUInt32(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ulong ReadUInt64(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 8);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public float ReadFloat(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 4);
            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte[] ReadBytes(IntPtr addr, int size)
        {
            var array = new byte[size];
            var lpNumberOfBytesRead = 1;
            Kernel32.ReadProcessMemory(ProcessHandle, addr, array, size, ref lpNumberOfBytesRead);
            return array;
        }

        public string ReadString(IntPtr addr, int maxLength = 32)
        {
            var bytes = ReadBytes(addr, maxLength * 2);

            int stringLength = 0;
            for (int i = 0; i < bytes.Length - 1; i += 2)
            {
                if (bytes[i] == 0 && bytes[i + 1] == 0)
                {
                    stringLength = i;
                    break;
                }
            }

            if (stringLength == 0)
            {
                stringLength = bytes.Length - (bytes.Length % 2);
            }

            return Encoding.Unicode.GetString(bytes, 0, stringLength);
        }

        public void WriteInt32(IntPtr addr, int val)
        {
            WriteBytes(addr, BitConverter.GetBytes(val));
        }

        public void WriteFloat(IntPtr addr, float val)
        {
            WriteBytes(addr, BitConverter.GetBytes(val));
        }

        public void WriteUInt8(IntPtr addr, byte val)
        {
            var bytes = new byte[] { val };
            WriteBytes(addr, bytes);
        }

        public void WriteByte(IntPtr addr, int value)
        {
            Kernel32.WriteProcessMemory(ProcessHandle, addr, new byte[] { (byte)value }, 1, 0);
        }

        public void WriteBytes(IntPtr addr, byte[] val)
        {
            Kernel32.WriteProcessMemory(ProcessHandle, addr, val, val.Length, 0);
        }

        public void WriteString(IntPtr addr, string value, int maxLength = 32)
        {
            var bytes = new byte[maxLength];
            var stringBytes = Encoding.Unicode.GetBytes(value);
            Array.Copy(stringBytes, bytes, Math.Min(stringBytes.Length, maxLength));
            WriteBytes(addr, bytes);
        }

        internal IntPtr FollowPointers(int[] offsets, bool readFinalPtr)
        {
            var ptr = ReadUInt64(BaseAddress + offsets[0]);

            for (int i = 1; i < offsets.Length - 1; i++)
            {
                ptr = ReadUInt64((IntPtr)ptr + offsets[i]);
            }

            var finalAddress =  (IntPtr) ptr + offsets[offsets.Length - 1];
            
            
            if (readFinalPtr) return (IntPtr)ReadUInt64(finalAddress);
            
            return finalAddress;
        }

        public void SetBitValue(IntPtr addr, byte flagMask, bool setValue)
        {
            byte currentByte = ReadUInt8(addr);
            byte modifiedByte;
    
            if (setValue)
                modifiedByte = (byte)(currentByte | flagMask);
            else
                modifiedByte = (byte)(currentByte & ~flagMask);
        
            WriteUInt8(addr, modifiedByte);
        }

        public bool IsGameLoaded()
        {
            var loadingCheckPtr = FollowPointers(new[] { Offsets.FileMan, Offsets.LoadingScreenFlag }, false);
            return ReadInt32(loadingCheckPtr) == 0;
            
        }
    }
}

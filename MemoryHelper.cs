using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SysProcess = System.Diagnostics.Process;

namespace GTA5_Casino_Helper
{
    public static class MemoryHelper
    {
        // TODO Support x86 ( Fork me plz,i'm lazy. )
        [DllImport("kernel32.dll")]
        public static extern IntPtr ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);
        public static byte[] ReadBytes(IntPtr Handle, IntPtr Address, uint BytesToRead)
        {
            IntPtr ptrBytesRead;
            byte[] buffer = new byte[BytesToRead];
            ReadProcessMemory(Handle, Address, buffer, BytesToRead, out ptrBytesRead);
            return buffer;
        }
        public static Int64 ReadInt64(IntPtr Address, uint length = 8, IntPtr? Handle = null)
        {
            return (BitConverter.ToInt64(ReadBytes((IntPtr)Handle, Address, length), 0));
        }
        static IntPtr GetBase(SysProcess handle, string module = null)
        {
            ProcessModuleCollection modules = handle.Modules;
            if (module != null)
            {
                for (int i = 0; i <= modules.Count - 1; i++)
                {
                    if (modules[i].ModuleName == module)
                    {
                        return (IntPtr)modules[i].BaseAddress;
                    }
                }
                Console.WriteLine("Module Not Found");

            }
            else
            {
                return (IntPtr)handle.MainModule.BaseAddress;
            }
            return (IntPtr)0;
        }
        static IntPtr GetBase(string pname)
        {
            SysProcess handle = SysProcess.GetProcessesByName(pname)[0];
            return handle.MainModule.BaseAddress;
        }
        public static IntPtr ReadValueByPrt(SysProcess process, IntPtr[] offsets, bool debug = false, string module = null)
        {

            IntPtr tmpptr = (IntPtr)0;
            IntPtr Base = GetBase(process);
            Console.WriteLine("Original base: " + Base);
            if (module != null)
            {
                Base = GetBase(process, module);
                Console.WriteLine("Module base: " + Base);
                Console.WriteLine("");

            }

            for (int i = 0; i <= offsets.Length - 1; i++)
            {
                if (i == 0)
                {
                    if (debug)
                        Console.Write(Base + "[Base] + " + offsets[i] + "[OFFSET 0]");
                    IntPtr ptr = IntPtr.Add(Base, (int)offsets[i]);
                    tmpptr = (IntPtr)ReadInt64(ptr, 8, process.Handle);
                    if (debug)
                        Console.WriteLine(" is " + tmpptr);
                }
                else
                {
                    if (debug)
                        Console.Write(tmpptr + " + " + offsets[i] + "[OFFSET " + i + "]");
                    IntPtr ptr2 = IntPtr.Add(tmpptr, (int)offsets[i]);
                    tmpptr = (IntPtr)ReadInt64(ptr2, 8, process.Handle); //last position's value

                    if (debug)
                        Console.WriteLine(" is " + tmpptr);
                }
            }

            return tmpptr;
        }
        public static IntPtr GetPtr(SysProcess process, IntPtr[] offsets, bool debug = false, string module = null)
        {
            IntPtr tmpptr = (IntPtr)0;
            IntPtr Base = GetBase(process);
            Console.WriteLine("Original base: " + Base);
            if (module != null)
            {
                Base = GetBase(process, module);
                Console.WriteLine("Module base: " + Base);
                Console.WriteLine("");

            }

            for (int i = 0; i <= offsets.Length - 1; i++)
            {
                if (i == 0)
                {
                    if (debug)
                        Console.Write(Base + "[Base] + " + offsets[i] + "[OFFSET 0]");
                    IntPtr ptr = IntPtr.Add(Base, (int)offsets[i]);
                    tmpptr = (IntPtr)ReadInt64(ptr, 8, process.Handle);
                    if (debug)
                        Console.WriteLine(" is " + tmpptr);
                }
                else
                {
                    if (debug)
                        Console.Write(tmpptr + " + " + offsets[i] + "[OFFSET " + i + "]");
                    IntPtr ptr2 = IntPtr.Add(tmpptr, (int)offsets[i]);
                    if (i == offsets.Length - 1)
                    {
                        if (debug)
                            Console.WriteLine(" is " + ptr2);
                        return ptr2;
                    }
                    tmpptr = (IntPtr)ReadInt64(ptr2, 8, process.Handle);

                    if (debug)
                        Console.WriteLine(" is " + tmpptr);
                }
            }

            return tmpptr;
        }
    }
}

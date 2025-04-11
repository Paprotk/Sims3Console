using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sims3Console
{
    public static unsafe class NativeExports
    {
        [UnmanagedCallersOnly(EntryPoint = "ICallSetup", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static void ICallSetup(delegate* unmanaged[Cdecl]<byte*, void*, void> addInternalCall)
        {
            var method1 = "Console::Create"u8; // Changed to ::
            var method2 = "Console::WriteLine"u8; // Changed to ::

            delegate* unmanaged[Stdcall]<void> createPtr = &ConsoleCreate;
            delegate* unmanaged[Stdcall]<sbyte*, void> writePtr = &ConsoleWriteLine;

            fixed (byte* pName1 = method1)
            fixed (byte* pName2 = method2)
            {
                addInternalCall(pName1, (void*)createPtr);
                addInternalCall(pName2, (void*)writePtr);
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "ConsoleCreate", CallConvs = new[] { typeof(CallConvStdcall) })]
        public static void ConsoleCreate()
        {
            if (GetConsoleWindow() == IntPtr.Zero)
                AllocConsole();
        }

        [UnmanagedCallersOnly(EntryPoint = "ConsoleWriteLine", CallConvs = new[] { typeof(CallConvStdcall) })]
        public static void ConsoleWriteLine(sbyte* utf8Str)
        {
            try
            {
                string? str = Marshal.PtrToStringUTF8((IntPtr)utf8Str);
                Console.WriteLine(str ?? "<null>");
            }
            catch (Exception ex)
            {
                File.AppendAllText("console_error.log", ex.ToString());
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
    }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// Build Script
// dotnet publish -c Release -r win-x86 /p:PlatformTarget=x86
namespace Sims3Console
{
    public static unsafe class NativeExports
    {
        [UnmanagedCallersOnly(EntryPoint = "ICallSetup", CallConvs = [typeof(CallConvCdecl)])]
        public static void ICallSetup(delegate* unmanaged[Cdecl]<byte*, void*, void> addInternalCall)
        {
            var method1 = "Console::Create"u8;
            var method2 = "Console::WriteLine"u8;
            var method3 = "Console::Close"u8;

            delegate* unmanaged[Stdcall]<void> createPtr = &ConsoleCreate;
            delegate* unmanaged[Stdcall]<sbyte*, void> writePtr = &ConsoleWriteLine;
            delegate* unmanaged[Stdcall]<void> closePtr = &ConsoleClose;

            fixed (byte* pName1 = method1)
            fixed (byte* pName2 = method2)
            fixed (byte* pName3 = method3)
            {
                addInternalCall(pName1, createPtr);
                addInternalCall(pName2, writePtr);
                addInternalCall(pName3, closePtr);
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "ConsoleCreate", CallConvs = [typeof(CallConvStdcall)])]
        public static void ConsoleCreate()
        {
            if (GetConsoleWindow() == IntPtr.Zero)
                AllocConsole();
        }
        
        [UnmanagedCallersOnly(EntryPoint = "ConsoleClose", CallConvs = [typeof(CallConvStdcall)])]
        public static void ConsoleClose()
        {
            if (GetConsoleWindow() != IntPtr.Zero)
                FreeConsole();
        }

        [UnmanagedCallersOnly(EntryPoint = "ConsoleWriteLine", CallConvs = [typeof(CallConvStdcall)])]
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
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
    }
}
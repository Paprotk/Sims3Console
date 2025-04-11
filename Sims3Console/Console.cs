using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sims3Console
{
    public static unsafe class NativeExports
    {
        private static Dictionary<string, StreamWriter> logWriters = new Dictionary<string, StreamWriter>();

        [UnmanagedCallersOnly(EntryPoint = "ICallSetup", CallConvs = [typeof(CallConvCdecl)])]
        public static void ICallSetup(delegate* unmanaged[Cdecl]<byte*, void*, void> addInternalCall)
        {
            var method1 = "Console::Create"u8;
            var method2 = "Console::WriteLine"u8;
            var method3 = "Console::Close"u8;
            var method4 = "Console::StartLogging"u8;
            var method5 = "Console::StopLogging"u8;

            delegate* unmanaged[Stdcall]<void> createPtr = &ConsoleCreate;
            delegate* unmanaged[Stdcall]<sbyte*, void> writePtr = &ConsoleWriteLine;
            delegate* unmanaged[Stdcall]<void> closePtr = &ConsoleClose;
            delegate* unmanaged[Stdcall]<sbyte*, void> startLoggingPtr = &ConsoleStartLogging;
            delegate* unmanaged[Stdcall]<sbyte*, void> stopLoggingPtr = &ConsoleStopLogging;

            fixed (byte* pName1 = method1)
            fixed (byte* pName2 = method2)
            fixed (byte* pName3 = method3)
            fixed (byte* pName4 = method4)
            fixed (byte* pName5 = method5)
            {
                addInternalCall(pName1, createPtr);
                addInternalCall(pName2, writePtr);
                addInternalCall(pName3, closePtr);
                addInternalCall(pName4, startLoggingPtr);
                addInternalCall(pName5, stopLoggingPtr);
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

                // Write to all active logs
                foreach (var writer in logWriters.Values)
                {
                    string datetime = DateTime.Now.ToString("dd MMM yyyy 'at' HH:mm");
                    writer.WriteLine($"[{datetime}] {str ?? "<null>"}");
                    writer.Flush();  // Flush after every write to ensure crash-proofing
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("console_error.log", ex.ToString());
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "ConsoleStartLogging", CallConvs = [typeof(CallConvStdcall)])]
        public static void ConsoleStartLogging(sbyte* filenameUtf8)
        {
            string filename = Marshal.PtrToStringUTF8((IntPtr)filenameUtf8) ?? "default_log";

            // Create a logs directory if it does not exist
            string logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            // Create the log file path inside the logs directory
            string logFileName = Path.Combine(logsDirectory, $"{filename}_{DateTime.Now:dd_MM_yyyy_HH_mm}_log.txt");

            // Add a new StreamWriter to the dictionary for the log file
            if (!logWriters.ContainsKey(logFileName))
            {
                logWriters[logFileName] = new StreamWriter(logFileName, append: true);
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "ConsoleStopLogging", CallConvs = [typeof(CallConvStdcall)])]
        public static void ConsoleStopLogging(sbyte* filenameUtf8)
        {
            string filename = Marshal.PtrToStringUTF8((IntPtr)filenameUtf8) ?? "default_log";

            // Construct the log file name (same as StartLogging)
            string logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            string logFileName = Path.Combine(logsDirectory, $"{filename}_{DateTime.Now:dd_MM_yyyy_HH_mm}_log.txt");

            // Stop logging for the specific file if it exists
            if (logWriters.ContainsKey(logFileName))
            {
                logWriters[logFileName].Close();
                logWriters.Remove(logFileName);
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

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Robomation.Standalone
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RobotSensoryDataCallback([MarshalAs(UnmanagedType.LPArray, SizeConst=RobotApi.PacketSize)] byte[] buf);

    static class RobotApi
    {
        public const int PacketSize = 20;

        private const string DllName = "roboid";

        public unsafe static string[] GetPorts()
        {
            IntPtr portNamesPtr;
            int count;
            robot_get_ports(out portNamesPtr, out count);

            char** pPortNames = (char**)portNamesPtr.ToPointer();
            var ports = new string[count];
            for (int i = 0; i < ports.Length; ++i) {
                ports[i] = Marshal.PtrToStringAnsi(new IntPtr(pPortNames[i]));
            }
            robot_free_ports(portNamesPtr, count);

            return ports;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void robot_get_ports(out IntPtr pPortNames, out int count);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void robot_free_ports(IntPtr portNames, int count);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_register")]
        public static extern void Register(int type, string name, string productId, int sensoryPacketType);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_create")]
        public static extern IntPtr Create();

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_create_port")]
        public static extern IntPtr CreatePort(string name);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_dispose")]
        public static extern void Dispose(IntPtr ptr);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_get_type")]
        public static extern int GetType(IntPtr ptr);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_get_address")]
        public static extern int GetAddress(IntPtr ptr, StringBuilder buffer, int size);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_is_connected")]
        public static extern int IsConnected(IntPtr ptr);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_set_sensory_data_callback")]
        public static extern void SetSensoryDataCallback(IntPtr ptr, RobotSensoryDataCallback cb);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_is_motoring_data_sent")]
        public static extern int IsMotoringDataSent(IntPtr ptr);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_set_motoring_data")]
        public static extern void SetMotoringData(IntPtr ptr, byte[] buf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LogCallback(string log);

        [DllImport(DllName, CallingConvention=CallingConvention.Cdecl, EntryPoint="robot_set_log_callback")]
        public static extern void SetLogCallback(LogCallback callback);
    }
}

#endif
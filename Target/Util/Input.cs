using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;

namespace Target {
    public static class Input {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] public static extern short GetKeyState(int nVirtKey);

        public static bool IsGameFocused {
            get {
                var activatedHandle = GetForegroundWindow();
                if(activatedHandle == IntPtr.Zero)
                    return false;

                var procId = Environment.ProcessId;
                _ = GetWindowThreadProcessId(activatedHandle, out var activeProcId);

                return activeProcId == procId;
            }
        }

        public static bool IsCtrlDown => IsGameFocused && (GetKeyState((int)VirtualKey.LCONTROL) & 0x80) != 0;
    }
}

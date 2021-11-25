using System.Runtime.InteropServices;

public class Fidget
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Press ESC to stop");
        do
        {
            var maxX = 800;
            var maxY = 600;
            var rand = new Random();
            while (!Console.KeyAvailable)
            {
                var nextX = rand.Next(maxX - (int)Math.Round(maxX * 0.25), maxX);
                var nextY = rand.Next(maxY - (int)Math.Round(maxY * 0.25), maxY);
                var p = new Win32.POINT(nextX, nextY);
                Win32.ClientToScreen(new IntPtr(), ref p);
                Win32.SetCursorPos(p.x, p.y);
                Win32.SetThreadExecutionState(Win32.EXECUTION_STATE.ES_DISPLAY_REQUIRED | Win32.EXECUTION_STATE.ES_CONTINUOUS);
                //Win32.SetThreadExecutionState(Win32.EXECUTION_STATE.ES_CONTINUOUS | Win32.EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
                Thread.Sleep(500);
            }
        } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
    }

    private class Win32
    {
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int X, int Y)
            {
                x = X;
                y = Y;
            }
        }
    }
}


using Fidget;
public static class Program
{
    const int MAX_STEP_DELAY = 10; //mS
    const int MIN_STEP_DELAY = 1; //mS
    const int MAX_X_PIXELS = 1920;
    const int MAX_Y_PIXELS = 1080;

    static int NumCycles;
    static int MinsToRun;
    static bool PreventScreenShutoff;
    static bool ShouldRadomize;
    static bool PreventPcSleep;
    static bool OptimizeForRunescape;

    static Win32Helper.POINT Point;
    static Random Rand = new Random();

    static DateTime StartTime;
    static DateTime EndTime;

    public static void Main(string[] args)
    {
        #region INPUT
        // duration
        Print("Enter minutes (int) to run: ");
        if (!int.TryParse(Console.ReadLine(), out var MinsToRun) || MinsToRun <= 0) return; // TODO: re-ask the question if invalid
        Print($"SET program to run for: {MinsToRun} minutes", ConsoleColor.Green, true);

        // move mouse every N seconds
        Print("Enter every N (int) seconds for mouse to move: ");
        if (!int.TryParse(Console.ReadLine(), out var fidgetN_Seconds) || fidgetN_Seconds <= 0) return; // TODO: re-ask the question if invalid
        Print($"SET cycle duration for every: {fidgetN_Seconds} seconds", ConsoleColor.Green);

        // optimize for Runescape?
        Print("Do you want to optimize for Runescape? y/N");
        OptimizeForRunescape = Console.ReadLine()?.ToLower() == "y";
        Print($"SET Runescape optimization: {(ShouldRadomize ? "ON" : "OFF")}", ConsoleColor.Green, true);
        if (OptimizeForRunescape) ShouldRadomize = true;
        else
        {
            // make timings random?
            Print("Add Randomization? y/N");
            ShouldRadomize = Console.ReadLine()?.ToLower() == "y";
            Print($"SET randomization: {(ShouldRadomize ? "ON" : "OFF")}", ConsoleColor.Green, true);

            // make timings random?
            Print("Do you want to prevent your screen from shutting off? y/N");
            PreventScreenShutoff = Console.ReadLine()?.ToLower() == "y";
            Print($"SET prevent screen shutoff: {(ShouldRadomize ? "ON" : "OFF")}", ConsoleColor.Green, true);

            // make timings random?
            Print("Do you want to prevent your PC from sleeping? y/N");
            PreventPcSleep = Console.ReadLine()?.ToLower() == "y";
            Print($"SET prevent PC sleep: {(ShouldRadomize ? "ON" : "OFF")}", ConsoleColor.Green, true);
        }
        #endregion

        Print("starting program...");
        StartTime = DateTime.Now;
        EndTime = StartTime.AddMinutes(MinsToRun);
        var prevX = 960;
        var prevY = 540;

        do
        {
            while (!Console.KeyAvailable)
            {
                int nextX = MAX_X_PIXELS / 2, nextY = MAX_Y_PIXELS / 2;
                if (OptimizeForRunescape)
                {
                    nextX = nextX > MAX_X_PIXELS / 2 ? Rand.Next(MAX_X_PIXELS - (int)Math.Round(MAX_X_PIXELS * 0.95), MAX_X_PIXELS / 2)
                        : Rand.Next(MAX_X_PIXELS / 2, MAX_X_PIXELS - (int)Math.Round(MAX_X_PIXELS * 0.05));
                    nextY = nextY > MAX_Y_PIXELS / 2 ? Rand.Next(MAX_Y_PIXELS - (int)Math.Round(MAX_Y_PIXELS * 0.60), MAX_Y_PIXELS / 2)
                        : Rand.Next(MAX_Y_PIXELS / 2, MAX_Y_PIXELS - (int)Math.Round(MAX_Y_PIXELS * 0.40));
                }
                else
                {
                    nextX = Rand.Next(MAX_X_PIXELS - (int)Math.Round(MAX_X_PIXELS * 0.95), MAX_X_PIXELS - (int)Math.Round(MAX_X_PIXELS * 0.05));
                    nextY = Rand.Next(MAX_Y_PIXELS - (int)Math.Round(MAX_Y_PIXELS * 0.60), MAX_Y_PIXELS - (int)Math.Round(MAX_Y_PIXELS * 0.40));
                }

                var deltaX = nextX - prevX;
                var deltaY = nextY - prevY;
                var ratioX = deltaX / (float)(deltaY < 0 ? deltaY * -1 : deltaY); // x's to move per y

                // calc positions
                var positions = new List<(int x, int y)>();
                var midPoint = (int)Math.Floor(deltaY / 2d);
                for (int i = 0; i <= (deltaY > 0 ? deltaY : deltaY * -1); i++)
                {
                    var x = (int)Math.Round(i * ratioX, 0) + prevX; // ratio takes care of direction  
                    var y = prevY + i * (deltaY > 0 ? 1 : -1); // deltaY sign takes care of direction
                    positions.Add((x, y));
                }
                prevX = nextX;
                prevY = nextY;

                // move the cursor
                positions.ForEach(p =>
                {
                    Thread.Sleep(Rand.Next(MIN_STEP_DELAY, MAX_STEP_DELAY));
                    Point = new Win32Helper.POINT(p.x, p.y);
                    Win32Helper.ClientToScreen(new IntPtr(), ref Point);
                    Win32Helper.SetCursorPos(Point.x, Point.y);
                });

                if (PreventScreenShutoff || OptimizeForRunescape) Win32Helper.SetThreadExecutionState(Win32Helper.EXECUTION_STATE.ES_DISPLAY_REQUIRED | Win32Helper.EXECUTION_STATE.ES_CONTINUOUS);
                else if (PreventPcSleep) Win32Helper.SetThreadExecutionState(Win32Helper.EXECUTION_STATE.ES_CONTINUOUS | Win32Helper.EXECUTION_STATE.ES_AWAYMODE_REQUIRED);

                // delay before next cycle
                var delayMs = (ShouldRadomize ? Rand.Next(0, fidgetN_Seconds) : fidgetN_Seconds) * 1000;
                if (delayMs <= 0) delayMs = Rand.Next(50, fidgetN_Seconds * 1000);

                // safety randomizations
                if (ShouldRadomize)
                {
                    for (int i = 0; i < Rand.Next(1, 5); i++)
                    {
                        delayMs += Rand.Next(1, 25);
                    }
                }

                NumCycles++;
                PrintInfo(delayMs);
                Thread.Sleep(delayMs);
            }
        } while (Console.ReadKey(true).Key != ConsoleKey.Escape && DateTime.Now.AddMinutes(MinsToRun) > StartTime);
        Console.WriteLine();
        Print("stopping program...", ConsoleColor.DarkYellow);
    }

    public static void Print(string msg, ConsoleColor color = ConsoleColor.White, bool includeExtraLine = false)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
        if (includeExtraLine) Console.WriteLine();
    }

    public static void PrintInfo(int delayMs)
    {
        Console.Clear();
        Console.WriteLine("Program running...\n");
        Print("Press ESC to stop.", ConsoleColor.DarkYellow);
        Print($"Disable PC Sleep: {IsEnabled(PreventPcSleep)}", ConsoleIsEnabled(PreventPcSleep));
        Print($"Disable Screen Shutoff: {IsEnabled(PreventScreenShutoff)}", ConsoleIsEnabled(PreventScreenShutoff));
        Print($"Randomization: {IsEnabled(ShouldRadomize)}", ConsoleIsEnabled(ShouldRadomize));
        Print($"Runescape Optimization: {IsEnabled(OptimizeForRunescape)}", ConsoleIsEnabled(OptimizeForRunescape));
        Print($"Pointer moved to: [{Point.x}, {Point.y}]");
        Print($"Next cycle starts in: {(decimal)Math.Round(delayMs / 1000d, 2)} seconds");
        Print($"Total cycles ran: {NumCycles}");
        Print($"Program termination at: {EndTime}");
    }

    private static string IsEnabled(bool enabled) => (enabled ? "ENABLED" : "DISABLED").ToUpper();
    private static ConsoleColor ConsoleIsEnabled(bool enabled) => enabled ? ConsoleColor.Green : ConsoleColor.Red;
}
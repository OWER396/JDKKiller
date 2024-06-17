using Microsoft.Win32;
using System.Diagnostics;

namespace JDKKiller
{
    internal static class Program
    {
        private static string? JDKPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Clickteam\\Fusion Developer 2.5\\General", "JDKDir", null)?.ToString();
        private static Process[]? JDKProcesses;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string interval = File.ReadAllText(AppContext.BaseDirectory + "\\.timerinterval");

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = Int32.Parse(interval);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
      
            ApplicationConfiguration.Initialize();
            Application.Run(new AppTray());

        }

        private static void timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now}] Timer tick");
            Console.WriteLine($"[{DateTime.Now}] Is CTF process running: {ProcessIsRunning("mmf2u")}");
            Console.WriteLine($"[{DateTime.Now}] Is JDK process running: {ProcessIsRunning(GetFilenameFromFilePath(JDKPath + "\\java.exe"))}");

            if (ProcessIsRunning(GetFilenameFromFilePath(JDKPath + "\\java.exe"))) JDKProcesses = Process.GetProcessesByName(GetFilenameFromFilePath(JDKPath + "\\java.exe"));

            if (!ProcessIsRunning("mmf2u") && ProcessIsRunning(GetFilenameFromFilePath(JDKPath + "\\java.exe")))
            {
                JDKProcesses?[0].Kill();
                Console.WriteLine("JDK process killed");
            }
        }

        private static string GetFilenameFromFilePath(string FullPath)
        {
            string? FilePath = Path.GetDirectoryName(FullPath);
            return Path.GetFileNameWithoutExtension(FullPath).ToLower();
        }

        private static bool ProcessIsRunning(string ProcessName)
        {
            Process[] processes = Process.GetProcessesByName(ProcessName);
            if (processes.Count() == 0) return false; else return true;
        }

    }

    public class AppTray : ApplicationContext
    {
        private NotifyIcon trayIcon;
        
        public AppTray()
        {
            trayIcon = new NotifyIcon()
            {
                Text = "JDKKiller",
                Icon = ResourceLoader.GetResource("Icon3.ico"),
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items = {new ToolStripMenuItem("Exit", null, Exit)}
                },
                Visible = true
            };
        }

        void Exit(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
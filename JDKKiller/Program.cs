using Microsoft.Win32;
using System.Diagnostics;
using System.Text.Json;

namespace JDKKiller
{
    internal static class Program
    {
        private static readonly string? JDKPath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Clickteam\\Fusion Developer 2.5\\General", "JDKDir", null)?.ToString()+"\\bin\\java.exe";
        private static readonly string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JDKKiller\\config.json";
        private static List<Process>? JDKProcesses;
        private static Config config;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if(!Directory.Exists(Path.GetDirectoryName(ConfigPath))) Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

            if(!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\JDKKiller\\config.json"))
            {
                config = new Config() { firstLaunch = true, timerInterval = 5000 };
                SaveConfig();
            }
            else LoadConfig();

            if (config.firstLaunch)
            {
                DialogResult dialogResult = MessageBox.Show("Would you like to have JDKKiller start every time your computer starts?", "JDKKiller", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if(dialogResult == DialogResult.Yes)
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rk.SetValue("JDKKiller", '"'+Process.GetCurrentProcess().MainModule.FileName+'"');
                }
                config.firstLaunch = false;
                SaveConfig();
            }
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = config.timerInterval;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
      
            ApplicationConfiguration.Initialize();
            Application.Run(new AppTray());

        }

        private static void timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (ProcessIsRunningFilename(JDKPath)) JDKProcesses = GetProcessByFilename(JDKPath);
                 
            if (!ProcessIsRunning("mmf2u") && ProcessIsRunningFilename(JDKPath))
            {
                JDKProcesses?[0].Kill();
                Console.WriteLine("JDK process killed");
            }
        }

        private static bool ProcessIsRunning(string ProcessName)
        {
            Process[] processes = Process.GetProcessesByName(ProcessName);
            if (processes.Count() == 0) return false; else return true;
        }

        private static List<Process> GetProcessByFilename(string filename)
        {
            List<Process> processes = new List<Process>();

            foreach (var process in Process.GetProcessesByName("java"))
            {
                if (process.MainModule?.FileName == filename)
                {
                    processes.Add(process);
                }
            }
            return processes;
        }

        private static bool ProcessIsRunningFilename(string filename)
        {
            foreach (var process in Process.GetProcessesByName("java"))
            {
                if (process.MainModule?.FileName == filename)
                {
                    return true;
                }
            }
            return false;
        }

        private static void LoadConfig()
        {
            config = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath));
        }

        private static void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config));
        }

    }

    class AppTray : ApplicationContext
    {
        private NotifyIcon trayIcon;
        
        public AppTray()
        {
            trayIcon = new NotifyIcon()
            {
                Text = "JDKKiller",
                Icon = ResourceLoader.GetIcon("Icon3.ico"),
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

    class Config
    {
        public bool firstLaunch { get; set; }
        public int timerInterval { get; set; }
    }
}
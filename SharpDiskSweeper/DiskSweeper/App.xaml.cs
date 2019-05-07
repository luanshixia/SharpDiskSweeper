using System.IO;
using System.Linq;
using System.Windows;

namespace DiskSweeper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string StartupPath;

        public void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0 && Directory.Exists(e.Args[0]))
            {
                App.StartupPath = e.Args[0];
            }

            //ShellExtensionAdder.AddRegEntries(ShellExtensionAdder.GetRegEntriesToAdd(
            //    shellObject: "Directory",
            //    appName: "SharpDiskSweeper",
            //    caption: "Open with SharpDiskSweeper",
            //    command: $"\"{typeof(App).Assembly.Location}\" \"%1\""));
        }
    }
}

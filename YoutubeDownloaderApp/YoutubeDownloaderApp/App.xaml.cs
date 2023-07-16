using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace YoutubeDownloaderApp
{
    public partial class App : Application
    {
        [STAThread]
        public static void a()
        {
            TaskScheduler.UnobservedTaskException += (sender, e) => e.SetObserved();

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}

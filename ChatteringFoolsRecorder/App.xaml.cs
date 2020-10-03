using ChatteringFoolsRecorder.AudioCapture;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChatteringFoolsRecorder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();

            var modules = new IModule[]
            {
                new AudioCapturePlugin()
            };

            var vm = new MainWindowViewModel(modules);
            mainWindow.DataContext = vm;
            mainWindow.Closing += (s, args) => vm.SelectedModule.Deactivate();
            mainWindow.Show();
        }
    }
}

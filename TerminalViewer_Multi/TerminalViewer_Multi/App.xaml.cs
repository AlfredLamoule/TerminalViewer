using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TerminalViewer;

namespace TerminalViewer_Multi
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(ConfigurationSettings.AppSettings["AccessMode"] == null)
            {
                MessageBox.Show("Impossible d'accéder aux paramètres de l'application !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            int accessValue = Convert.ToInt32(ConfigurationSettings.AppSettings["AccessMode"]);
            // Dev Mode
            if(accessValue == 2)
            {
                MainWindow main = new MainWindow();
                main.Show();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace TerminalViewer
{
    /// <summary>
    /// Logique d'interaction pour ShowCavsDialog.xaml
    /// </summary>
    public partial class ShowCavsDialog : Window
    {
        private TerminalView view;

        public GPSPoint newGPSCav {get; set;}
        public ObservableCollection<GPSPoint> gpsPoints { get; set; }

        public ShowCavsDialog(TerminalView view, ObservableCollection<GPSPoint> gpsPoints)
        {
            this.view = view;
            this.DataContext = this;
            
            InitializeComponent();

            this.gpsPoints = gpsPoints;
            this.listViewGPS.ItemsSource = this.gpsPoints;
        }

        private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox == null)
                return;

            GPSPoint gpsPoint = listBox.SelectedItem as GPSPoint;

            if (gpsPoint == null)
                return;

            this.newGPSCav = null;
            this.newGPSCav = gpsPoint;

            this.txt_CavName.Text = gpsPoint.Name;
            this.txt_ComPort.Text = gpsPoint.PortName;

            e.Handled = true;
        }

        private void btn_Ajouter_Click(object sender, RoutedEventArgs e)
        {
            this.gpsPoints.Add(new GPSPoint(view, txt_CavName.Text, txt_ComPort.Text, 0.0, 0.0));
        }

    }


}

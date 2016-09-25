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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TerminalViewer
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TerminalView view;

        public String SelectedTerminal { get; set; }
        public double drawSizeX { get; set; }
        public double drawSizeY { get; set; }

        public MainWindow()
        {
            Application.Current.MainWindow = this;

            InitializeComponent();

            this.MapContainer.NavigationUIVisibility = NavigationUIVisibility.Hidden;

        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            this.view = new TerminalView("TDF", 500, 200, 110, 5);

            if (view.ChargerMap("TDF"))
            {
                this.view.ViewMapTerminal();
                this.MapContainer.Content = this.view;
            }
            else
            {
                MessageBox.Show("Impossible de charger la map", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.DataContext = this.view;
            //this.savedPosGrid.ItemsSource = this.view.model.savedGPSPoints;
        }

        private void OnClose(object sender, EventArgs e)
        {
            this.view.unload();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            this.view.OnSaveButtonClick();
        }

        private void showCavs_Click(object sender, RoutedEventArgs e)
        {
            this.view.OnShowCavsClick();
        }
        
        private void ClearHistory()
        {
            if (!this.MapContainer.CanGoBack && !this.MapContainer.CanGoForward)
            {
                return;
            }

            var entry = this.MapContainer.RemoveBackEntry();
            while (entry != null)
            {
                entry = this.MapContainer.RemoveBackEntry();
            }

            this.MapContainer.Navigate(new PageFunction<string>() { RemoveFromJournal = true });
        }

        private void gpsButtonDisable(object sender, RoutedEventArgs e)
        {
            this.view.OnGPSDisabled();
        }

        private void gpsButtonEnable(object sender, RoutedEventArgs e)
        {
            //this.btn_EnableGPS.IsChecked = this.view.EnableGPS();
        }

        private void btn_LoadMoves_Click(object sender, RoutedEventArgs e)
        {
            this.view.OnMovesLoaded();
        }

        private void OnGPSDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((DataGrid)e.Source).CurrentItem as GPSPoint;
            
            if(item != null)
                this.view.trackGPS(item);
        }

        private void OnResize(object sender, SizeChangedEventArgs e)
        {
            this.drawSizeX = MapContainer.ActualWidth;
            this.drawSizeY = MapContainer.ActualHeight;
        }

        private void Combo_Terminal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            String terminal = this.Combo_Terminal.SelectedValue.ToString();

            if (terminal != null && terminal.CompareTo("") != 0)
            {
                this.view.unload();
                this.view = null;
                this.view = new TerminalView(terminal, 500, 200, 110, 5);

                if (view.ChargerMap(terminal))
                {
                    this.view.ViewMapTerminal();

                    this.MapContainer.Content = null;
                    this.MapContainer.Content = this.view;
                }

            }
            
        }

        private void Button_LoadMoves_Click(object sender, RoutedEventArgs e)
        {
            this.view.OnMovesLoaded();
        }

        private void Button_AddPoint_Click(object sender, RoutedEventArgs e)
        {
            Context_AddPoint context = new Context_AddPoint();
            if (context.ShowDialog() == true)
            {
                this.view.OnAddPoint(context.point);
                
            }
        }

        private void Button_Pointing_Checked(object sender, RoutedEventArgs e)
        {
            this.view.OnPointing();
        }
    }

    public class DebugConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                Console.WriteLine("In converter: null");
                return value;
            }

            //Console.WriteLine("In converter: " + value.GetType().ToString());
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
        #endregion
    }

    public static class Commands
    {
        public static readonly DependencyProperty DataGridDoubleClickProperty =
          DependencyProperty.RegisterAttached("DataGridDoubleClickCommand", typeof(ICommand), typeof(Commands),
                            new PropertyMetadata(new PropertyChangedCallback(AttachOrRemoveDataGridDoubleClickEvent)));

        public static ICommand GetDataGridDoubleClickCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DataGridDoubleClickProperty);
        }

        public static void SetDataGridDoubleClickCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DataGridDoubleClickProperty, value);
        }

        public static void AttachOrRemoveDataGridDoubleClickEvent(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DataGrid dataGrid = obj as DataGrid;
            if (dataGrid != null)
            {
                ICommand cmd = (ICommand)args.NewValue;

                if (args.OldValue == null && args.NewValue != null)
                {
                    dataGrid.MouseDoubleClick += ExecuteDataGridDoubleClick;
                }
                else if (args.OldValue != null && args.NewValue == null)
                {
                    dataGrid.MouseDoubleClick -= ExecuteDataGridDoubleClick;
                }
            }
        }

        private static void ExecuteDataGridDoubleClick(object sender, MouseButtonEventArgs args)
        {
            DependencyObject obj = sender as DependencyObject;
            ICommand cmd = (ICommand)obj.GetValue(DataGridDoubleClickProperty);
            if (cmd != null)
            {
                if (cmd.CanExecute(obj))
                {
                    cmd.Execute(obj);
                }
            }
        }

    }
}
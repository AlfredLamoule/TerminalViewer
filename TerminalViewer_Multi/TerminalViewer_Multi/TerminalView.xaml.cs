
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
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Threading;
using TerminalViewer.Classes;

namespace TerminalViewer
{
    /// <summary>
    /// Logique d'interaction pour TerminalView.xaml
    /// </summary>
    public partial class TerminalView : Page
    {
        private static readonly double BASE_StrokeThickness = 0.8;

        private bool running = true;

        // Block à modifier
        public Block targetBlock { get; set; }

        // TEST
        //private static readonly String terminalTDF = @"C:\_Applications\Config\TDF.txt";
        private String terminalTDF;// = @"\\lhesrvdata\psp$\GMP\TECHNIQUE\DSI\SYSTEMES DE PRODUCTION\PRODUCTION\Cavalier\Alexandre\Applis\Arborescence_complete_C\_Applications\Config\TDF.txt";
        private String terminalTNORD;// = @"\\lhesrvdata\psp$\GMP\TECHNIQUE\DSI\SYSTEMES DE PRODUCTION\PRODUCTION\Cavalier\Alexandre\Applis\Arborescence_complete_C\_Applications\Config\TNORD.txt";

        public BlockModel model { get; set; }

        // DATABASE
        private Database database;
        
        // GPS POINT
        //private SerialGPS serialGPS;
        private GPSPoint gps;

        //private Thread threadGPS;

        // ZOOM / TRANSLATE
        private Point pressedMouse;
        private GPSPoint trackingPoint;

        // MAPPING
        
        private String terminal;
        private int translationX;
        private int translationY;
        
        private ProcessBlocks processBlocks;
        private ProcessMapping processMapping;


        private readonly BackgroundWorker worker = new BackgroundWorker();

        public TerminalView(String terminal, int translationX, int translationY, int resize, int marge)
        {
            InitializeComponent();

            this.model = new BlockModel();

            this.targetBlock = new Block("Test");

            this.model.ActualCursor = Cursors.Arrow;
            this.trackingPoint = null;

            this.database = new Database("lhfsrvcavtest", "ConvertLogs", "Log2015");
            
            this.terminalTDF = @"E:\GMP\Terminaux\TDF.txt";
            this.terminalTNORD = @"E:\GMP\Terminaux\TNORD.txt";

            this.translationX = translationX;
            this.translationY = translationY;

            this.terminal		= terminal;

	        this.processBlocks		= new ProcessBlocks();
	        this.processMapping	    = new ProcessMapping(translationX, translationY, resize, marge);

	        this.BuildMap();
	        this.processMapping.CalculateMargin();
            
            this.ViewMapTerminal();
            
        }

        public bool ChargerMap(String terminal)
        {
            String file = getTerminalFile();

            if (!File.Exists(file))
                return false;

            List<double> mapBorders = this.processBlocks.LoadBlocksFile(file);

            this.BuildMap();

            this.terminal = terminal;

            return true;

        }

        public String getTerminalFile()
        {
            String fichier = "";

            if (String.Compare(terminal, "TNORD") == 0)
                fichier = terminalTNORD;
            else
                fichier = terminalTDF;

            return fichier;
        }

#region Build
        void BuildMap()
        {
            this.processMapping.BuildMatrixTerminal(terminal, 1.0, this.translationX, this.translationY);
        }

        void BuildBlock(Block block, int width, int height)
        {
            List<double> coords = block.getCoords();
            double latitude = block.getLatitude();
            double longitude = block.getLongitude();
            int sizeX = width;
            int sizeY = height;
            double ratio = 0.7;

            this.processMapping.BuildMatrixBlock(coords, latitude, longitude, sizeX, sizeY, ratio);
        }
#endregion Build

#region View

        public void ViewMapTerminal()
        {
            this.BuildMap();

            this.DrawGlobalMap();

        }


#endregion View

#region Drawing
        void DrawGlobalMap()
        {
            this.model.FillBlocksCollection(this.GetBlocksCollection(processBlocks.getAllBlocks()));

            this.DataContext = this.model;

            this.testItems.ItemsSource = this.model.AllItems;
        }

        void DrawBlockSlots(Block block)
        {
            this.model.FillSlotsCollection(this.GetSlotsCollection(block));

            
        }

        void DrawAllVisibleSlots()
        {

        }

#endregion Drawing

#region GetCollection
    public ObservableCollection<Block> GetBlocksCollection(List<Block> listeBlocks)
    {
        ObservableCollection<Block> blocksCollection = new ObservableCollection<Block>();

        foreach (Block block in listeBlocks)
        {
            Point[] points = this.convertGPSCoords(block.getFormattedCoords());

            Polygon poly = new Polygon();

            foreach (Point point in points)
                poly.Points.Add(point);

            block.Points = poly.Points;

            block.Name = block.getName();
            //block.Fill = Brushes.LightGreen;
            block.StrokeThickness = BASE_StrokeThickness;
            block.Stroke = Brushes.Black;
            blocksCollection.Add(block);

            Console.WriteLine(block.Name);
        }

        return blocksCollection;
    }

    public ObservableCollection<Slot> GetSlotsCollection(Block block)
    {
        ObservableCollection<Slot> slotsCollection = new ObservableCollection<Slot>();

        int minTravee = block.getFirstTravee();
        int maxTravee = minTravee + block.getNbTravees() - 1;

        int minSlot = block.getFirstSlot();
        int maxSlot = minSlot + block.getNbSlots() - 1;

        if (minSlot == 0 && maxSlot == 0)
            return slotsCollection;

        for (int trav = minTravee; trav <= maxTravee; trav++)
        {
            List<double[]> coordsTrav = this.processBlocks.getTraveeCoords(block, trav);
            //for (int s = minSlot ; s <= maxSlot; s++)
            for (int s = maxSlot; s >= minSlot; s--)
            {
                List<double[]> coordsSlot = this.processBlocks.getSlotCoords(block, coordsTrav, s);
                Point[] points = this.convertGPSCoords(coordsSlot);
                Polygon poly = new Polygon();

                foreach (Point point in points)
                    poly.Points.Add(point);

                String slotName = String.Format("{0:000}{1}{2:00}", trav, block.getName().Substring(3, 2), s);
                Slot newSlot = new Slot(block, slotName, poly.Points, Brushes.LightGreen, Brushes.Black);

                slotsCollection.Add(newSlot);
            }
        }

        return slotsCollection;
    }
#endregion GetCollection

#region Calculs

    // CALCULS 
        Point[] convertGPSCoords(List<double[]> blockCoords)
        {
            Point[] points = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                Point point = convertGPSPoint(blockCoords[i][0], blockCoords[i][1]);
                points[i] = new Point(point.X, point.Y);
            }

            return points;
        }

        public Point convertGPSPoint(double lat, double lon)
        {
            return this.processMapping.ConvertGPSToPixel(lat, lon);
        }

        public Point convertPixelToGPS(Point point)
        {
            double[] coords = this.processMapping.ConvertPixelToGPS(point);
            return new Point(coords[0], coords[1]);
        }

#endregion Calculs

#region EVENTS
        
        private void Block_MouseEnter(object sender, MouseEventArgs e)
        {
            var poly = sender as Polygon;
            var block = poly.DataContext as Block;
            
            
        }

        private Point lastMousePosition;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ItemsPresenter itemsPresenter = GetVisualChild<ItemsPresenter>(testItems);
            Canvas itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Canvas;

            itemsPanel.CaptureMouse();

            pressedMouse = e.GetPosition(itemsPanel);

            Canvas canvas = sender as Canvas;
            this.lastMousePosition = e.GetPosition(canvas);

        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            
            ItemsPresenter itemsPresenter = GetVisualChild<ItemsPresenter>(testItems);
            Canvas itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Canvas;

            if (itemsPanel.IsMouseCaptured)
            {
                if (trackingPoint != null)
                    disengageTracking();
                
                var transform = itemsPanel.RenderTransform as MatrixTransform;

                Vector delta = Point.Subtract(e.GetPosition(itemsPanel), pressedMouse); // delta from old mouse to current mouse

                var matrix = transform.Matrix;
                var translate = new TranslateTransform(delta.X, delta.Y);
                matrix = translate.Value * matrix;

                itemsPanel.RenderTransform = new MatrixTransform(matrix); //transform;
                e.Handled = true;

            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            ItemsPresenter itemsPresenter = GetVisualChild<ItemsPresenter>(testItems);
            Canvas itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Canvas;

            itemsPanel.ReleaseMouseCapture();

        }

        private void OnScroll(object sender, MouseWheelEventArgs e)
        {
            // Canvas
            var element = sender as UIElement;
            var position = e.GetPosition(element);
            var transform = element.RenderTransform as MatrixTransform;
            var matrix = transform.Matrix;
            var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            if (trackingPoint != null)
                matrix.ScaleAtPrepend(scale, scale, trackingPoint.CenterPoint.X, trackingPoint.CenterPoint.Y);
            else
                matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);

            element.RenderTransform = new MatrixTransform(matrix);
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void Block_Click(object sender, MouseButtonEventArgs e)
        {
            // Double click on block
            if (e.ClickCount == 2)
            {
                Polygon poly = (Polygon)sender;
                var block = poly.DataContext as Block;

                if (block != null)
                {
                    //var block = e.Argument as Block;

                    if (this.model.SelectedBlock != null)
                        resetBlockStyle(this.model.SelectedBlock);

                    this.model.SelectedBlock = block;
                    block.Selected = true;

                    block.StrokeThickness = 0.0;

                    this.targetBlock = block;

                    DrawBlockSlots(block);

                }
            }
        }

        public double getDistance(Block block, Point point)
        {
            double minDistance = 99999;
            Point nearestPoint = new Point();

            foreach (Double[] coords in block.getFormattedCoords())
            {
                Point blockPoint = new Point(coords[0], coords[1]);
                double distance = this.processMapping.GetDistance(point, blockPoint);
                if (distance < minDistance)
                {
                    nearestPoint = blockPoint;
                    minDistance = distance;
                }
            }

            return minDistance;
        }
        
        // MENU CONTEXT
        private void PolygonProperties_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = e.Source as MenuItem;

            Block block = menuItem.DataContext as Block;

            if (block != null) 
            {
                BlockProperties blockProp = new BlockProperties(block);
                blockProp.ShowDialog();
                if (blockProp.DialogResult == true)
                    showModifiedBlock(blockProp.block);
            }
        }

        private void PolygonSelect_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = e.Source as MenuItem;

            Block block = menuItem.DataContext as Block;

            if (block != null)
            {
                block.Stroke = Brushes.Red;
                block.StrokeThickness = 10;
            }
        }

        private void BlockDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = e.Source as MenuItem;

            Block block = menuItem.DataContext as Block;

            if (block != null)
                if (block.isTemporary)
                    this.model.RemoveBlock(block);
        }

        private void SlotProperties_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = e.Source as MenuItem;
            var slot = menuItem.DataContext as Slot;
            Block block = slot.parent;

            if (block != null)
            {
                BlockProperties blockProp = new BlockProperties(block);
                blockProp.ShowDialog();
                if (blockProp.DialogResult == true)
                    showModifiedBlock(blockProp.block);
            }
        }

        private void SlotDeleteBlock_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = e.Source as MenuItem;
            var slot = menuItem.DataContext as Slot;
            Block block = slot.parent;

            if (block != null)
                if (block.isTemporary)
                    this.model.RemoveBlock(block);
        }

        private void GPSPointSavePos_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = e.Source as MenuItem;
            var gpsPoint = menuItem.DataContext as GPSPoint;

            if (gpsPoint != null)
            {
                double lat = gpsPoint.Latitude;
                double lon = gpsPoint.Longitude;

                BlockProperties blockProp = new BlockProperties(this.targetBlock, lat, lon);
                blockProp.ShowDialog();
                if (blockProp.DialogResult == true)
                    showModifiedBlock(blockProp.block);
            }
        }


        #endregion EVENTS

#region WindowCalls

        public void OnSaveButtonClick()
        {
            saveMapModifications();
        }

        // MAP
        public void OnPointing()
        {
            
        }
        
        public void OnAddPoint(CustomPoint point)
        {
            Point convertedPoint = this.convertGPSPoint(point.Latitude, point.Longitude);

            ActionPoint ap = new ActionPoint(point.Name, convertedPoint.X, convertedPoint.Y);
            this.model.AddActionPoint(ap);
        }

        public void OnShowCavsClick()
        {
            //var points = this.model.gpsPoints as List<GPSPoint>;

            List<GPSPoint> points = new List<GPSPoint>(this.model.gpsPoints.Cast<GPSPoint>());

            ShowCavsDialog dialog = new ShowCavsDialog(this, this.model.gpsPoints);
            dialog.Show();
        }

        public bool OnGPSEnabled()
        {
            foreach (GPSPoint gps in this.model.gpsPoints)
            {
                if (!gps.start())
                    MessageBox.Show("Impossible d'ouvrir le GPS du " + gps.Name, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return true;
        }


        public void OnGPSDisabled()
        {
            List<Thread> threadsToWait = new List<Thread>();

            foreach (GPSPoint gps in this.model.gpsPoints)
                if(gps.Connected)
                    threadsToWait.Add(gps.stop());

            if (threadsToWait.Count != 0)
                foreach (Thread thread in threadsToWait)
                    thread.Join();

            //this.model.gpsPoints.Clear();
        }

        public void OnMovesLoaded()
        {
            DateTime date = DateTime.Parse("01/08/2016");
            List<ActionPoint> points = this.database.getLiftDropFromDate(date);

            foreach (ActionPoint point in points)
            {
                point.Position = this.convertGPSPoint(point.Position.Y, point.Position.X);
            }

            this.model.FillActionPointsCollection(new ObservableCollection<ActionPoint>(points));
        }

        public void trackGPS(GPSPoint gpsPoint)
        {
            engageTracking(gpsPoint);
        }

        private void gpsPoint_PositionChanged(Object sender, EventArgs e)
        {
            // Changement de position
            var gpsPoint = sender as GPSPoint;

            Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate
            {

                ItemsPresenter itemsPresenter = GetVisualChild<ItemsPresenter>(testItems);
                Canvas itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Canvas;

                MainWindow main = Application.Current.MainWindow as MainWindow;


                Point windowCenter = new Point(main.drawSizeX / 2, main.drawSizeY / 2);
                Point canvasCenter = main.MapContainer.TransformToDescendant(itemsPanel).Transform(windowCenter);

                var transform = itemsPanel.RenderTransform as MatrixTransform;

                Vector delta = Point.Subtract(canvasCenter, gpsPoint.CenterPoint); // delta from old mouse to current mouse

                var matrix = transform.Matrix;
                var translate = new TranslateTransform(delta.X, delta.Y);
                matrix = translate.Value * matrix;

                itemsPanel.RenderTransform = new MatrixTransform(matrix); //transform;
            }); 


        }

#endregion WindowCalls

        // TRACKING
        private void engageTracking(GPSPoint gpsPoint)
        {
            if (gpsPoint.Connected)
            {
                foreach (GPSPoint _gpsPoint in this.model.gpsPoints)
                    _gpsPoint.PropertyChanged -= gpsPoint_PositionChanged;

                gpsPoint.PropertyChanged += gpsPoint_PositionChanged;

                this.trackingPoint = gpsPoint;

            }
        }

        private void disengageTracking()
        {
            foreach (GPSPoint _gpsPoint in this.model.gpsPoints)
                _gpsPoint.PropertyChanged -= gpsPoint_PositionChanged;

            this.trackingPoint = null;
        }
        //

        private void showModifiedBlock(Block block)
        {
            Block checkBlock = getModifiedBlock(block);

            if (checkBlock != null)
                this.model.RemoveBlock(checkBlock);

            block.isTemporary = true;
            //block.Fill = new SolidColorBrush(Colors.Green);

            Point[] points = this.convertGPSCoords(block.getFormattedCoords());

            Polygon poly = new Polygon();

            foreach (Point point in points)
                poly.Points.Add(point);

            block.Points = poly.Points;

            this.model.AddModifiedBlock(block);
        }

        private Block getModifiedBlock(Block block)
        {
            foreach (Block b in this.model.BlockItems)
                if (b.isTemporary && block.Name.CompareTo(b.Name) == 0)
                    return b;

            return null;
        }

        private void resetBlockStyle(Block block)
        {
            block.Selected = false;
            block.StrokeThickness = BASE_StrokeThickness;
        }

        public bool isRunning()
        {
            return this.running;
        }

        public void unload()
        {
            List<Thread> threadsToWait = new List<Thread>();

            foreach (GPSPoint gps in this.model.gpsPoints)
                if (gps.Connected)
                    threadsToWait.Add(gps.stop());

            if (threadsToWait.Count != 0)
                foreach (Thread thread in threadsToWait)
                    thread.Join();
        }

        private void saveMapModifications()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Sauvegarder la map";
            saveFileDialog.FileName = terminal;
            saveFileDialog.Filter = "Fichiers texte (*.txt)|*.txt";

            List<Block> tmp_Blocks = new List<Block>();

            if (saveFileDialog.ShowDialog() == true)
                for (int i = 0; i < this.model.BlockItems.Count; i++)
                {
                    Block b = this.model.BlockItems[i];
                    if (b.isTemporary)
                        if (this.processBlocks.SaveBlockCoords(this.terminal, this.getTerminalFile(), saveFileDialog.FileName, b))
                            tmp_Blocks.Add(b);
                }

            if (tmp_Blocks.Count != 0)
                this.model.ApplyBlocks(tmp_Blocks);
        }

        public void gpsProblem(String reason)
        {
            MessageBox.Show("Problème GPS : " + reason, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void savePosition(GPSPoint gpsPoint)
        {

        }

    }

}

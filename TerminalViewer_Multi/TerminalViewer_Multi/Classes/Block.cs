using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Input;

namespace TerminalViewer
{
    public class BlockModel : INotifyPropertyChanged
    {
        public Block SelectedBlock { get; set; }

        public Cursor ActualCursor;

        // boiler-plate
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public ObservableCollection<Block> BlockItems { get; set; }
        public ObservableCollection<Slot> SlotsItems { get; set; }
        public ObservableCollection<GPSPoint> gpsPoints { get; set; }
        public ObservableCollection<ActionPoint> ActionPoints { get; set; }
        public CompositeCollection AllItems { get; set; }

        private CollectionContainer actionPointsContainer;

        public BlockModel()
        {
            this.ActualCursor = Cursors.Cross;

            this.BlockItems = new ObservableCollection<Block>();
            this.SlotsItems = new ObservableCollection<Slot>();
            this.gpsPoints = new ObservableCollection<GPSPoint>();
            this.ActionPoints = new ObservableCollection<ActionPoint>();
            //this.savedGPSPoints = new ObservableCollection<GPSPoint>();

            AllItems = new CompositeCollection();
            CollectionContainer cont1 = new CollectionContainer() { Collection = BlockItems };
            CollectionContainer cont2 = new CollectionContainer() { Collection = SlotsItems };
            CollectionContainer cont3 = new CollectionContainer() { Collection = gpsPoints  };
            actionPointsContainer = new CollectionContainer() { Collection = ActionPoints };
            AllItems.Add(cont1);
            AllItems.Add(cont2);
            AllItems.Add(cont3);
            AllItems.Add(actionPointsContainer);
        }

        public void FillBlocksCollection(ObservableCollection<Block> blocksCollection)
        {
            this.BlockItems.Clear();
            foreach (Block block in blocksCollection)
                this.BlockItems.Add(block);

        }

        public void FillSlotsCollection(ObservableCollection<Slot> slotsCollection)
        {
            this.SlotsItems.Clear();
            foreach (Slot slot in slotsCollection)
                this.SlotsItems.Add(slot);
        }

        public void FillActionPointsCollection(ObservableCollection<ActionPoint> actionPoints)
        {
            Console.WriteLine("Start FillActionPointsCollection");

            this.AllItems.Remove(this.actionPointsContainer);
            actionPointsContainer = new CollectionContainer() { Collection = actionPoints };
            
            /*
            this.ActionPoints.Clear();
            foreach (ActionPoint point in actionPoints)
                this.ActionPoints.Add(point);
            */

            this.AllItems.Add(actionPointsContainer);
            Console.WriteLine("End FillActionPointsCollection");
        }

        public void AddActionPoint(ActionPoint point)
        {
            point.PropertyChanged += new PropertyChangedEventHandler(point_PropertyChanged);
            this.ActionPoints.Add(point);
        }

        public void AddGPSPoint(GPSPoint point)
        {
            point.PropertyChanged += new PropertyChangedEventHandler(point_PropertyChanged);
            this.gpsPoints.Add(point);
        }

        public void AddSavedPoint(GPSPoint point)
        {
            //this.savedGPSPoints.Add(point);
        }

        public void RemoveSavedPoint(GPSPoint point)
        {

        }

        public void AddModifiedBlock(Block block)
        {
            this.BlockItems.Add(block);
        }

        public void RemoveBlock(Block block)
        {
            this.BlockItems.Remove(block);
        }

        public void ApplyBlocks(List<Block> blocks)
        {
            foreach (Block b in blocks)
                if (ApplyBlock(b))
                    b.isTemporary = false;
        }

        public bool ApplyBlock(Block block)
        {
            foreach (Block b in this.BlockItems)
                if(b.Name.CompareTo(block.Name) == 0) {
                    this.BlockItems.Remove(b);
                    return true;
                }

            return false;
        }

        private void point_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(gpsPoints);
            view.Refresh();
        }

    }

    public class Block
    {
        // DRAWING
        public PointCollection Points { get; set; }
        public Geometry Geometry { get; set; }
        public Brush Fill { get; set; }
        public Brush Stroke { get; set; }
        public String Name { get; set; }
        public double StrokeThickness { get; set; }

        // Types de blocks
        public const int UNKNOWN    = 0;
        public const int NORMAL     = 1;
        public const int ZONE       = 2;
        public const int PORTIQUE   = 3;

        // Noms
        public String name { get; set;}
        private String realName;

        // Propriétés blocks
        private int blockType;
        private int firstTravee;
        private int firstSlot;
        public int nbTravees { get; set; }
        public int nbSlots { get; set; }
        public bool Selected { get; set; }

        // Coordonnées
        private double latitude;
        private double longitude;
        public List<double> coords { get; set; }

        // Temp
        public bool isTemporary {get; set;}

        public Block(String name)
        {
            this.isTemporary = false;

            this.name = name;
            this.realName = name;

            this.blockType = UNKNOWN;

            this.firstSlot = 0;
            this.firstTravee = 0;

            this.initBlock();   
        }

        // Recopie
        public Block(Block block)
        {
            this.Name = block.Name;
            this.Points = block.Points;
            this.Fill = block.Fill;
            this.Stroke = block.Stroke;

            this.name = block.name;
            this.realName = block.realName;

            this.blockType = block.blockType;
            this.firstTravee = block.firstTravee;
            this.firstSlot = block.firstSlot;
            this.nbTravees = block.nbTravees;
            this.nbSlots = block.nbSlots;

            this.latitude = block.latitude;
            this.longitude = block.longitude;
            this.coords = block.coords;

            this.isTemporary = block.isTemporary;
        }

        // Spécial slots
        public Block(String name, PointCollection points, Brush fill, Brush stroke)
        {
            this.Name = name;
            this.Points = points;
            this.Fill = fill;
            this.Stroke = stroke;
        }

        private void initBlock()
        {
            // Block Type
            String patternNormal    = "^[0-9]{3}[A-Z]{2}";
            String patternZone      = "^Z[0-9]{5}";
            String patternPortique  = "QUAY";

            Regex regex = new Regex(patternNormal);
            Match matchNormal = regex.Match(this.name);

	        regex = new Regex(patternZone);
	        Match matchZone = regex.Match(this.name);

	        regex = new Regex(patternPortique);
	        Match matchPortique = regex.Match(this.name);

            if (matchNormal.Success)
                this.blockType = NORMAL;
            else if (matchZone.Success)
                this.blockType = ZONE;
            else if (matchPortique.Success)
                this.blockType = PORTIQUE;

            
	        // First Travee + Slot
	        if(this.blockType == NORMAL) {
		        // 111XXX22
		        // Travée 1 ==> 111
		        // RealName ==> XXX
		        // Slot 1 ==> 22

		        String strTravee = this.name.Substring(0, 3);
		        String strSlot   = this.name.Substring(6, 2);
		        this.realName	  = this.name.Substring(3, 3);

		        Int32.TryParse(strTravee, out this.firstTravee);
		        Int32.TryParse(strSlot,   out this.firstSlot);
	        }
        }

        // SETTERS
        public void setPosition(List<double> coordsList)
        {
            this.coords = coordsList;
        }

        public void setCenter(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public void setSize(int nbTravees, int nbSlots)
        {
            this.nbTravees = nbTravees;
            this.nbSlots = nbSlots;
        }

        public bool isMultiple()
        {
            return this.nbSlots > 1 || this.nbTravees > 1;
        }

        // GETTERS
        public List<double[]> getFormattedCoords()
        {
            List<double[]> tempCoords = new List<double[]>();

            tempCoords.Add(new double[2] { this.coords[0], this.coords[1] });
            tempCoords.Add(new double[2] { this.coords[2], this.coords[3] });
            tempCoords.Add(new double[2] { this.coords[4], this.coords[5] });
            tempCoords.Add(new double[2] { this.coords[6], this.coords[7] });

            return tempCoords;
        }

        public int getNbTravees()
        {
            return this.nbTravees;
        }

        public int getNbSlots()
        {
            return this.nbSlots;
        }

        public int getBlockType() {
	        return this.blockType;
        }

        public int getFirstTravee() {
	        return this.firstTravee;
        }

        public int getFirstSlot() {
	        return this.firstSlot;
        }

        public List<double> getCoords() {
	        return this.coords;
        }

        public String getName() {
	        return this.name;
        }

        public String getRealName() {
	        return this.realName;
        }

        public double getLatitude() {
	        return this.latitude;
        }

        public double getLongitude() {
	        return this.longitude;
        }

    }
}

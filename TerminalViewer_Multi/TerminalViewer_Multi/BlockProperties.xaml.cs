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

namespace TerminalViewer
{
    /// <summary>
    /// Logique d'interaction pour BlockProperties.xaml
    /// </summary>
    public partial class BlockProperties : Window
    {
        public Block block { get; set; }
        private double lat;
        private double lon;

        public BlockProperties(Block block)
        {
            InitializeComponent();

            this.block = block;
            
            fillInputs();
        }

        public BlockProperties(Block block, double lat, double lon)
        {
            InitializeComponent();

            this.block = block;
            this.lat = lat;
            this.lon = lon;

            this.btn_BasDroite.Visibility = System.Windows.Visibility.Visible;
            this.btn_HautDroite.Visibility = System.Windows.Visibility.Visible;
            this.btn_BasGauche.Visibility = System.Windows.Visibility.Visible;
            this.btn_HautGauche.Visibility = System.Windows.Visibility.Visible;

            fillInputs();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            int nbSlots;
            int nbTravees;
            try
            {
                nbSlots = Convert.ToInt32(this.txt_nbSlots.Text);
                nbTravees = Convert.ToInt32(this.txt_nbTravees.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur s'est produite lors de la récupération des données", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<double> coords = getCoordsList();

            if(coords == null) 
            {
                MessageBox.Show("Une erreur s'est produite lors de la récupération des coordonnées", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            this.block = new Block(this.block);

            this.block.nbSlots = nbSlots;
            this.block.nbTravees = nbTravees;
            this.block.coords = coords;


            this.Close();
        }

        private List<double> getCoordsList()
        {
            List<double> list = new List<double>();
            
            try
            {
                list.Add(Convert.ToDouble(this.txt_Latitude1.Text));
                list.Add(Convert.ToDouble(this.txt_Longitude1.Text));
                list.Add(Convert.ToDouble(this.txt_Latitude2.Text));
                list.Add(Convert.ToDouble(this.txt_Longitude2.Text));
                list.Add(Convert.ToDouble(this.txt_Latitude3.Text));
                list.Add(Convert.ToDouble(this.txt_Longitude3.Text));
                list.Add(Convert.ToDouble(this.txt_Latitude4.Text));
                list.Add(Convert.ToDouble(this.txt_Longitude4.Text));
            }
            catch (Exception e)
            {
                return null;
            }

            return list;
        }

        private void fillInputs()
        {
            this.txt_nbSlots.Text = block.nbSlots.ToString();
            this.txt_nbTravees.Text = block.nbTravees.ToString();

            this.txt_Latitude1.Text = block.coords[0].ToString();
            this.txt_Latitude2.Text = block.coords[2].ToString();
            this.txt_Latitude3.Text = block.coords[4].ToString();
            this.txt_Latitude4.Text = block.coords[6].ToString();
            this.txt_Longitude1.Text = block.coords[1].ToString();
            this.txt_Longitude2.Text = block.coords[3].ToString();
            this.txt_Longitude3.Text = block.coords[5].ToString();
            this.txt_Longitude4.Text = block.coords[7].ToString();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void OnBasDroite_Click(object sender, RoutedEventArgs e)
        {
            this.txt_Latitude3.Text = this.lon.ToString();
            this.txt_Longitude3.Text = this.lat.ToString();
        }

        private void OnHautDroite_Click(object sender, RoutedEventArgs e)
        {
            this.txt_Latitude4.Text = this.lon.ToString();
            this.txt_Longitude4.Text = this.lat.ToString();
        }

        // 2
        private void OnBasGauche_Click(object sender, RoutedEventArgs e)
        {
            this.txt_Latitude2.Text = this.lon.ToString();
            this.txt_Longitude2.Text = this.lat.ToString();
        }

        // 1
        private void OnHautGauche_Click(object sender, RoutedEventArgs e)
        {
            this.txt_Latitude1.Text = this.lon.ToString();
            this.txt_Longitude1.Text = this.lat.ToString();
        }

        
    }

}

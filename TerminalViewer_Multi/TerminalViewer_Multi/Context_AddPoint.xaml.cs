﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using TerminalViewer.Classes;

namespace TerminalViewer
{
    /// <summary>
    /// Logique d'interaction pour Context_AddPoint.xaml
    /// </summary>
    public partial class Context_AddPoint : Window
    {
        public String Name { get; set; }
        public String Test { get; set; }
        public CustomPoint point { get; set; }
        public Context_AddPoint()
        {
            InitializeComponent();

            this.DataContext = this;
            
            point = new CustomPoint();
        }

        public Context_AddPoint(Point pos)
        {
            InitializeComponent();

            point = new CustomPoint();
            
            this.point.Latitude = pos.X;
            this.point.Longitude = pos.Y;
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        
    }
    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return value.ToString();
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string data = value as string;
            if (data == null)
            {
                return value;
            }
            if (data.Equals(string.Empty))
            {
                return 0;
            }
            if (!string.IsNullOrEmpty(data))
            {
                decimal result;
                //Hold the value if ending with .
                if (data.EndsWith(".") || data.Equals("-0"))
                {
                    return Binding.DoNothing;
                }
                if (decimal.TryParse(data, out result))
                {
                    return result;
                }
            }
            return Binding.DoNothing;
        }
    }
}

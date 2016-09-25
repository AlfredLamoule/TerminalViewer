using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Threading;

namespace TerminalViewer
{
    public class GPSPoint : INotifyPropertyChanged
    {
        public String Name { get; set; }
        public String PortName { get; set; }

        private bool _Connected;
        
        public bool Connected {
            get { return _Connected; }
            set
            {
                _Connected = value;
                NotifyPropertyChanged("Connected");
            }
        }

        private SerialGPS serialGPS;
        private Thread threadGPS;
        private TerminalView view;
        
        private Point _CenterPoint;
        public Point CenterPoint {
            get { return _CenterPoint; }
            set
            {
                _CenterPoint = value;
                NotifyPropertyChanged("CenterPoint");
            }
        }

        public double Latitude { get;set;}
        public double Longitude { get; set; }
        
        public GPSPoint(TerminalView view, String Name, String serialPort, double centerX, double centerY)
        {
            this.Name = Name;
            this.PortName = serialPort;
            this.Connected = false;
            this.view = view;

            this.serialGPS = new SerialGPS(this, serialPort);
            this.CenterPoint = new Point(centerX, centerY);   
        }

        public bool start()
        {
            if (this.serialGPS.initGPS())
            {
                threadGPS = new Thread(new ThreadStart(this.serialGPS.startReceive));

                threadGPS.Start();

                this.Connected = true;
            }

            return this.Connected;
        }

        public Thread stop()
        {
            this.Connected = false;
            return this.threadGPS;
        }

        public void OnPositionReceived(double[] pos)
        {
            Point point = this.view.convertGPSPoint(pos[0], pos[1]);
            Point pointGPS = new Point(pos[0], pos[1]);

            this.Latitude = pos[1];
            this.Longitude = pos[0];
            this.CenterPoint = point;


        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

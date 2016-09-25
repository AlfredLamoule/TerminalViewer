using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace TerminalViewer
{
    public class ActionPoint : INotifyPropertyChanged
    {
        // LIFT / DROP
        public String Event { get; set; }

        // POSITION
        private Point _Position;
        public Point Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                NotifyPropertyChanged("Position");
            }
        }

        public ActionPoint(String Event, double lat, double lon) {
            this.Event = Event;
            this._Position = new Point(lat, lon);
        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

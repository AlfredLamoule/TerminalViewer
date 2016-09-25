using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TerminalViewer
{
    class DataGPS
    {
        
        public String Name { get; set; }
        public Point Position { get; set; }

        public DataGPS(String name, Point position)
        {
            this.Name = name;
            this.Position = position;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TerminalViewer
{
    public class Slot
    {
        public Block parent { get; set; }
        public String slotname { get; set; }

        public PointCollection Points { get; set; }
        public Brush Fill { get; set; }
        public Brush Stroke { get; set; }
        
        public Slot(Block block, String slotname, PointCollection points, Brush fill, Brush stroke)
        {
            this.parent = block;                
            this.slotname = slotname;

            this.Points = points;
            this.Fill = fill;
            this.Stroke = stroke;
        }
    }
}

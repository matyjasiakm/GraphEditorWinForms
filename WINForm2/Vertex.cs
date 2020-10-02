using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace WINForm2
{
    public class Vertex
    {
        public float X { set; get; }
        public float Y { set; get; }
        public Color Color { set; get; }

        public Vertex(int x, int y, Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }
}

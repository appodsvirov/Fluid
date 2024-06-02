using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    public class GraphNode
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Velocity { get; set; }

        public GraphNode(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
            Velocity = 0.0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    public class GraphEdge
    {
        public GraphNode FromNode { get; set; }
        public GraphNode ToNode { get; set; }
        public double Flow { get; set; }

        public GraphEdge(GraphNode fromNode, GraphNode toNode)
        {
            FromNode = fromNode;
            ToNode = toNode;
            Flow = 0.0;
        }
    }
}

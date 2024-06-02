using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    public class HydrodynamicGraph
    {
        public List<GraphNode> Nodes { get; private set; }
        public List<GraphEdge> Edges { get; private set; }
        private const double Pi = Math.PI;

        public HydrodynamicGraph()
        {
            Nodes = new List<GraphNode>();
            Edges = new List<GraphEdge>();
        }

        public void AddNode(GraphNode node)
        {
            Nodes.Add(node);
        }

        public void AddEdge(GraphNode fromNode, GraphNode toNode)
        {
            var edge = new GraphEdge(fromNode, toNode);
            Edges.Add(edge);
        }

        public void CalculateFlows()
        {
            foreach (var edge in Edges)
            {
                double dx = edge.ToNode.X - edge.FromNode.X;
                double dy = edge.ToNode.Y - edge.FromNode.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                edge.Flow = CalculateFlow(edge.FromNode.Velocity, edge.ToNode.Velocity, distance);
            }
        }

        private double CalculateFlow(double v1, double v2, double distance)
        {
            return (v1 + v2) / (2 * distance);
        }

        public void UpdateVelocities()
        {
            foreach (var node in Nodes)
            {
                var incidentEdges = Edges.Where(e => e.FromNode == node || e.ToNode == node);
                node.Velocity = incidentEdges.Sum(e => e.Flow) / incidentEdges.Count();
            }
        }
    }
}

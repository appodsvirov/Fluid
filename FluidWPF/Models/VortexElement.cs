using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    public class VortexElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Intensity { get; set; }

        public VortexElement(double x, double y, double intensity)
        {
            X = x;
            Y = y;
            Intensity = intensity;
        }
    }
}

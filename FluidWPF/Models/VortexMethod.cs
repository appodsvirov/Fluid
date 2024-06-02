using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    public class VortexMethod
    {
        private const double Pi = Math.PI;

        public List<VortexElement> VortexElements { get; set; }

        public VortexMethod()
        {
            VortexElements = new List<VortexElement>();
        }

        public void AddVortexElement(double x, double y, double intensity)
        {
            VortexElements.Add(new VortexElement(x, y, intensity));
        }

        public (double u, double v) CalculateVelocity(double x, double y)
        {
            double u = 0;
            double v = 0;

            foreach (var element in VortexElements)
            {
                double dx = x - element.X;
                double dy = y - element.Y;
                double rSquared = dx * dx + dy * dy;
                double factor = element.Intensity / (2 * Pi * rSquared);

                u += factor * dy;
                v -= factor * dx;
            }

            return (u, v);
        }

        public double CalculatePressure(double x, double y, double density)
        {
            var (u, v) = CalculateVelocity(x, y);
            return 0.5 * density * (u * u + v * v);
        }
    }
}

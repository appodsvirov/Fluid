using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    public class BaldwinLomaxModel
    {
        private const double APlus = 26;
        private const double CCp = 1.6;
        private const double CKleb = 0.3;
        private const double CWk = 0.25;
        private const double k = 0.4;
        private const double K = 0.0168;
        private int _numY;
        private double _density;
        private double _nu;
        private double _dy;
        private Dictionary<(double, int), double> CashSolveMuInner = new Dictionary<(double, int), double>();
        private Dictionary<(double, int), double> CashSolveMuOuter = new Dictionary<(double, int), double>();

        public BaldwinLomaxModel(Fluid fluid)
        {
            _numY = fluid.numY;
            _density = fluid.density;
            _nu = 1E-03;
            _dy = fluid.h;
        }

        public double SolveDynamicSpeed( double u1, double u2, int j, double[] u, double[] v)
        {
            double du = Math.Abs(u1 - u2);
            du = Math.Round(du, 5);
            int y = j; 
            return Math.Sqrt(SolveTurbulentStress(du, y, u, v) / _density);
        }

        // закон Стейна и Рейна
        private double SolveTurbulentStress(double du, int y, double[] u, double[] v)
        {
            return _density * SolveTurbulentViscosity(du, y, u, v) * du / _dy;
        }

        private double SolveTurbulentViscosity(double du, int y, double[] u, double[] v)
        {
            //return SolveMuInner(du, y);
            double yCrossover = SolveYCrossover(du, u, v);
            return (y <= yCrossover) ? SolveMuInner(du, y): SolveMuOuter(du, y, u, v); 
        }

        private int SolveYCrossover(double du, double[] u, double[] v)
        {
            for (int i = 0; i < _numY; i++)
            {
                if (Math.Abs(SolveMuInner(du, i) - SolveMuOuter(du, i, u, v)) < 1E-05)
                {
                    return i;
                }
            }
            return _numY;
        }

        // Формула Прандтля – Ван Дриста
        private double SolveMuInner(double du, int y)
        {
            if(CashSolveMuInner.ContainsKey((du, y)))
            {
                return CashSolveMuInner[(du, y)];
            }
            double AbsOmega = SolveAbsOmega(du);
            double l = SolveMixingLength(y, SolveYPlus(du, y));

            var result = _density * l * l * AbsOmega;

            CashSolveMuInner.Add((du, y), result);
            return result;
        }

        //длина пути смешения Прандтля
        private double SolveMixingLength(int y, double yPlus)
        {
            return k * y * (1 - Math.Exp(-yPlus / APlus));
        }
        private double SolveYPlus(double du, int y)
        {

            var result = y * Math.Sqrt(_density * (du / _dy) / _nu);
            return result;
        }

        //функция завихренности скорости
        private double SolveAbsOmega(double du)
        {
            // В случае простейшего течения вблизи пластины
            // без градиента давления во внешнем потоке
            // приближенно полагают
            return Math.Abs(du / _dy);
        }

        private double SolveMuOuter(double du, int y, double[] u, double[] v)
        {
            if (CashSolveMuOuter.ContainsKey((du, y)))
            {
                return CashSolveMuOuter[(du, y)];
            }
            (double FMax, double YMax) = SolveFYMax(du);
            var result = _density * K * CCp * FWake(FMax, YMax, u, v) * SolveFKleb(y, YMax);
            CashSolveMuOuter.Add((du, y), result);
            return result;
        }
        private static double SolveFKleb(int y, double YMax)
        {
            return 1 / (1 + 5.5 * Math.Pow(CKleb * y / YMax, 6));
        }

        private double FWake(double FMax, double YMax, double[] u, double[] v)
        {
            return Math.Min(YMax * FMax,
                CWk * YMax * Math.Pow(SolveUDif(u, v), 2) / FMax);
        }

        private Dictionary<double, (double, double)> CashSolveFYMax = new ();
        private (double, double) SolveFYMax(double du)
        {
            if (CashSolveFYMax.ContainsKey(du))
            {
                return CashSolveFYMax[du];
            }
            double YMax = double.MinValue;
            for(int i = 0; i < _numY; i++)
            {
                var res = SolveFY(du, i);
                if (res > YMax) YMax = res;
            }
            var result = (SolveFY(du, (int)YMax), YMax);
            CashSolveFYMax.Add(du, result);
            return result;
        }

        private Dictionary<(double, int), double> CashSolveFY = new();
        private double SolveFY(double du, int y)
        {
            if(CashSolveFY.ContainsKey((du, y)))
            {
                return CashSolveFY[(du, y)];
            }
            var result = y * SolveAbsOmega(du) * (1 - Math.Exp(-SolveYPlus(du, y) / APlus)); //_dy
            CashSolveFY.Add((du, y), result);
            return result;
        }

        private static double SolveUDif(double[] u, double[] v)
        {
            double maxSpeed = double.MinValue;
            double minSpeed = double.MaxValue;

            for (int i = 0; i < u.Length; i++)
            {
                double speed = Math.Sqrt(u[i] * u[i] + v[i] * v[i]);

                if (speed > maxSpeed)
                {
                    maxSpeed = speed;
                }

                if (speed < minSpeed)
                {
                    minSpeed = speed;
                }
            }

            return maxSpeed - minSpeed;
        }

    }
}

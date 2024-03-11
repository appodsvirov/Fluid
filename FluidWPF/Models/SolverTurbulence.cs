using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    public class SolverTurbulence
    {
        private const double APlus = 26;
        private const double CCp = 1.6;
        private const double CKleb = 0.3;
        private const double CWk = 0.25;
        private const double k = 0.4;
        private const double K = 0.0168;

        public static double SolveDynamicSpeed(double density,
                                         double du,
                                         double dy,
                                         double y,
                                         double nu)
        {
            return Math.Sqrt(SolveTurbulentStress(density, du, dy, y, nu) / density);
        }

        // закон Стейна и Рейна
        public static double SolveTurbulentStress(double density,
                                         double du,
                                         double dy,
                                         double y,
                                         double nu)
        {
            return density * SolveTurbulentViscosity(density, du, dy, y, nu) * du/dy;
        }

        public static double SolveTurbulentViscosity(
                                         double density,
                                         double du,
                                         double dy,
                                         double y,
                                         double nu)
        {
            var muInner = SolveMuInner(density, du, dy, y, nu);
            return muInner;
            //return (y <= yCrossover) ? muInner : muOuter; 
        }

        public static double SolveCrossover()
        {
            return 1;//to do
        }

        // Формула Прандтля – Ван Дриста
        public static double SolveMuInner(double density,
                                   double du, 
                                   double dy,
                                   double y,
                                   double nu
            )
        {
            double AbsOmega = SolveAbsOmega(du, dy);
            double l = SolveMixingLength(y, SolveYPlus(y, density, nu, du, dy));
            return density * l* l * AbsOmega;
        }

        //длина пути смешения Прандтля
        public static double SolveMixingLength(double y, double yPlus)
        {
            return k * y * (1 - Math.Exp( -yPlus / APlus));
        }

        public static double SolveYPlus(double y, double roW, double nuW, double duW, double dyW)
        {
            return y * Math.Sqrt(roW * (duW / dyW) / nuW);
        }

        //функция завихренности скорости
        public static double SolveAbsOmega(double du, double dy /*, double omega = 1*/)
        {
            // В случае простейшего течения вблизи пластины
            // без градиента давления во внешнем потоке
            // приближенно полагают
            return Math.Abs(du / dy);

            // Вообще надо так находить
            //return Math.Sqrt(2 * omega * omega);
        }
        
        public static double SolveOmegaIJ (double dUi, double dUj, double dx)
        {
            return 0.5 * (dUi/dx - dUj/dx);
        }

        //        public static double SolveMuOuter(double density,
        //                                   double du,
        //                                   double dy,
        //                                   double y,
        //                                   double nu)
        //        {
        //            return density * K * CCp * FWake() * SolveFKleb(y);
        //        }
        //        public static double SolveFKleb(double y)
        //        {
        //            return 1 / (1 + 5.5 * Math.Pow(CKleb * y / SolveYMax(), 6));
        //        }

        //        public static double FWake()
        //        {
        //            return Math.Min(SolveYMax()*SolveFMax(),CWk*SolveYMax()*SovleUDif()*SolveUDif()/FMax() );
        //        }

        //        public static double SolveFY()
        //        {

        //        }

        //        public static double SolveUDif()
        //        {

        //        }

    }
}

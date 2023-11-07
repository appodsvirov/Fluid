using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid_Full_
{
    public static class LogFluid
    {
        public static List<Point> Coordinates { get; set; }
        public static List<List<(double, double, double)>> Speeds { get; set; }

        public static Point GetCoordinate(int idCoordinate) => Coordinates[idCoordinate];

        public static void AddPoint(int x, int y)
        {
            Coordinates.Add(new Point(x, y));
            Speeds.Add(new List<(double, double, double)>());
        }
        
        public static void AddSpeeds(int idCoordinate, double U, double V)
        {
            double speed = Math.Sqrt(U*U + V*V);
            Speeds[idCoordinate].Add((U, V, speed));

        }
        static LogFluid()
        {
            Coordinates = new List<Point>();
            Speeds  = new List<List<(double, double, double)>>();
        }

        public static void GetFullSpeed()
        {
            for (int i = 0; i < Coordinates.Count; i++)
            {
                var list = Speeds[i];
                using (StreamWriter streamWriter = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\FullSpeed_" + i.ToString() + "_.txt"))
                {
                    for(int j = 0; j < list.Count; j++)
                    {
                        streamWriter.WriteLine(list[j].Item3) ;
                    }
                }
            }
        }
    }
}

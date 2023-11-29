using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid_Full_
{
    public static class LogFluid
    {
        public static List<Point> Coordinates { get; set; }
        public static List<List<double>> Speeds { get; set; }
        public static List<List<double>> Mass { get; set; }
        public static List<List<double>> P { get; set; }

        public static StreamWriter StreamWriterSpeedUp { get; set; }
        public static StreamWriter StreamWriterSpeedDown { get; set; }
        public static StreamWriter StreamWriterMassUp { get; set; }
        public static StreamWriter StreamWriterMassDown { get; set; }
        public static StreamWriter StreamWriterPUp { get; set; }
        public static StreamWriter StreamWriterPDown { get; set; }


        public static Point GetCoordinate(int idCoordinate) => Coordinates[idCoordinate];

        public static void AddPoint(int x, int y)
        {
            Coordinates.Add(new Point(x, y));
            Speeds.Add(new List<double>());
            Mass.Add(new List<double>());
            P.Add(new List<double>());
        }

        public static void AddSpeeds(int idCoordinate, double U, double V)
        {
            double speed = Math.Sqrt(U * U + V * V);
            Speeds[idCoordinate].Add(speed);

        }
        public static void AddMass(int idCoordinate, double m)
        {
            Mass[idCoordinate].Add(m);
        }
        public static void AddP(int idCoordinate, double p)
        {
            P[idCoordinate].Add(p);
        }
        static LogFluid()
        {
            Coordinates = new List<Point>();
            Speeds = new List<List<double>>();
            Mass = new List<List<double>>();
            P = new List<List<double>>();

            StreamWriterSpeedUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\FullSpeed_0_.txt");
            StreamWriterSpeedDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\FullSpeed_1_.txt");


            StreamWriterMassUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\Mass_0_.txt");
            StreamWriterMassDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\Mass_1_.txt");


            StreamWriterPUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\P_0_.txt");
            StreamWriterPDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\P_1_.txt");
        }

        public static void Dispose()
        {
            StreamWriterSpeedUp.Close();
            StreamWriterSpeedDown.Close();
            StreamWriterMassUp.Close();
            StreamWriterMassDown.Close();
            StreamWriterPUp.Close();
            StreamWriterPDown.Close();
        }

        public static void GetFullSpeed()
        {
            StreamWriterSpeedUp.WriteLine(string.Join("\n", Speeds[0]));
            StreamWriterSpeedDown.WriteLine(string.Join("\n", Speeds[1]));
        }

        public static void GetMass()
        {
            StreamWriterMassUp.WriteLine(string.Join("\n", Mass[0]));
            StreamWriterMassDown.WriteLine(string.Join("\n", Mass[1]));
        }

        public static void GetP()
        {
            StreamWriterPUp.WriteLine(string.Join("\n", P[0]));
            StreamWriterPDown.WriteLine(string.Join("\n", P[1]));
        }
    }
}

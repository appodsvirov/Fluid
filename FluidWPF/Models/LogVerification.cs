﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidWPF.Models
{
    internal class LogVerification
    {
        StreamWriter sWPUp, sWMUp, sWUUp;
        StreamWriter sWPDown, sWMDown, sWUDown;

        StringBuilder sBPUp, sBMUp, sBUUp;
        StringBuilder sBPDown, sBMDown, sBUDown;

        public (int, int) pointUp ;
        public (int, int) pointDown ;

        //public (int, int) pointUp = ((int)(83 * ScaleNet), (int)(52 * ScaleNet));
        //public (int, int) pointDown = ((int)(83 * ScaleNet), (int)(48 * ScaleNet));

        public LogVerification(int ScaleNet = 1)
        {
            pointUp =   ((int)(99 * ScaleNet), (int)(62 * ScaleNet));
            pointDown = ((int)(99 * ScaleNet), (int)(38 * ScaleNet));
            sBPUp = new StringBuilder();
            sBPDown = new StringBuilder();
            sBMUp = new StringBuilder();
            sBMDown = new StringBuilder();
            sBUUp = new StringBuilder();
            sBUDown = new StringBuilder();
        }

        public void Log(double p1, double p2, double m1, double m2, double u1, double u2)
        {
            sBPUp.Append(p1).AppendLine();
            sBPDown.Append(p2).AppendLine();
            sBMUp.Append(m1).AppendLine();
            sBMDown.Append(m2).AppendLine();
            sBUUp.Append(u1).AppendLine();
            sBUDown.Append(u2).AppendLine();
        }

        public void Log(double p1)
        {
            sBPUp.Append(p1).AppendLine();
        }

        public void GetLog(string path = "P.txt")
        {
            using (var sWPUp = new StreamWriter("Data/Simple/" + path))
            {
                sWPUp.Write(sBPUp.ToString());
            }
        }

        public void GetFullLog()
        {
            using (var sWPUp = new StreamWriter("Data/Full/P_Up.txt"))
            using (var sWPDown = new StreamWriter("Data/Full//P_Down.txt"))
            using (var sWMUp = new StreamWriter("Data/Full//M_Up.txt"))
            using (var sWMDown = new StreamWriter("Data/Full//M_Down.txt"))
            using (var sWUUp = new StreamWriter("Data/Full//U_Up.txt"))
            using (var sWUDown = new StreamWriter("Data/Full//U_Down.txt"))
            {
                sWPUp.Write(sBPUp.ToString());
                sWPDown.Write(sBPDown.ToString());
                sWMUp.Write(sBMUp.ToString());
                sWMDown.Write(sBMDown.ToString());
                sWUUp.Write(sBUUp.ToString());
                sWUDown.Write(sBUDown.ToString());
            }
        }
    }
}


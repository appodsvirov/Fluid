using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fluid
{
    public class LogFluid
    {
        StreamWriter sWPUp, sWMUp, sWUUp;
        StreamWriter sWPDown, sWMDown, sWUDown;

        StringBuilder sBPUp, sBMUp, sBUUp;
        StringBuilder sBPDown, sBMDown, sBUDown;

        public (int, int) pointUp = (99, 62);
        public (int, int) pointDown = (99, 38);

        public LogFluid()
        {
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

        public void GetLog()
        {
            try
            {
                sWPUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\fluid\\output\\P_Up.txt");
                sWPDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\fluid\\output\\P_Down.txt");
                sWMUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\fluid\\output\\M_Up.txt");
                sWMDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\fluid\\output\\M_Down.txt");
                sWUUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\fluid\\output\\U_Up.txt");
                sWUDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\fluid\\output\\U_Down.txt");


                sWPUp.Write(sBPUp.ToString());
                sWPDown.Write(sBPDown.ToString());
                sWMUp.Write(sBMUp.ToString());
                sWMDown.Write(sBMDown.ToString());
                sWUUp.Write(sBUUp.ToString());
                sWUDown.Write(sBUDown.ToString());
            }
            finally
            {
                sWPUp.Dispose();
                sWPDown.Dispose();
                sWMUp.Dispose();
                sWMDown.Dispose();
            }
        }
    }
}

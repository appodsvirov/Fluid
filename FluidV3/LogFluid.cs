using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidV3
{
    public class LogFluid
    {
        StreamWriter sWPUp, sWMUp, sWUUp;
        StreamWriter sWPDown, sWMDown, sWUDown;

        StringBuilder sBPUp, sBMUp, sBUUp;
        StringBuilder sBPDown, sBMDown, sBUDown;

        public (int, int) pointUp = ((int)(99*GlobalValues.scaleNet), (int)(62*GlobalValues.scaleNet));
        public (int, int) pointDown = ((int)(99*GlobalValues.scaleNet), (int)(38*GlobalValues.scaleNet));

        //public (int, int) pointUp = ((int)(83 * GlobalValues.scaleNet), (int)(52 * GlobalValues.scaleNet));
        //public (int, int) pointDown = ((int)(83 * GlobalValues.scaleNet), (int)(48 * GlobalValues.scaleNet));

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

        public void Log(double p1)
        {
            sBPUp.Append(p1).AppendLine();
        }

        public void GetLog()
        {
            using(var sWPUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\P_Up.txt"))
            using(var sWPDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\P_Down.txt"))
            using(var sWMUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\M_Up.txt"))
            using(var sWMDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\M_Down.txt"))
            using(var sWUUp = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\U_Up.txt"))
            using(var sWUDown = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\U_Down.txt"))
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

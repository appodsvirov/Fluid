using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid_Full_
{
    internal class GlobalCanvas
    {
        private int width;
        private int height;
        private double simHeight;
        private double cScale;
        private double simWidth;
        private double cnt; // to do: int or double?

        private Scene scene;

        public GlobalCanvas()
        {
            width = 1500;
            height = 900;
            simHeight = 1;
            simWidth = width / cScale;
            cScale = height / simHeight;
            cnt = 0;
            scene = new Scene();
        }

        public int cX(int x)
        {
            return (int) (x * cScale); // to do: int or double?
        }
        public int cY(int y)
        {
            return (int)(height - y * cScale); // to do: int or double?
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fluid
{
    public static class Scene
    {
        #region Поля
        public static double gravity;
        public static double dt;
        public static int numIters;
        public static int frameNr;
        public static double overRelaxation;
        public static double obstacleX;
        public static double obstacleY;
        public static bool paused;
        public static int sceneNr;
        public static bool showPressure;
        public static bool showSmoke;
        public static Fluid fluid;
        public static bool Speed;
        public static double a;
        public static int obstacle;
        public static double x_late;
        public static double y_late;
        #endregion


        static Scene()
        {
            gravity = -9.81f;
            dt = 1.0f / 120.0f;
            numIters = 100;
            frameNr = 0;
            overRelaxation = 1.9f;
            obstacleX = 0.0f;
            obstacleY = 0.0f;
            paused = false;
            sceneNr = 0;
            showPressure = false;
            showSmoke = true;
            fluid = new Fluid(1000, 100, 100, 100);
            Speed = false;
            a = 0.0f;
            obstacle = 0;
            x_late = 0.0f;
            y_late = 0.0f;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Fluid_Full_
{
    public class Scene
    {
        #region Поля
        public double gravity;
        public double dt;
        public int numIters;
        public int frameNr;
        public double overRelaxation;
        public double obstacleX;
        public double obstacleY;
        public bool paused;
        public int sceneNr;
        public bool showPressure;
        public bool showSmoke;
        public Fluid fluid;
        public bool speed;
        public double a;
        public int obstacle;
        public double x_late;
        public double y_late;
        #endregion



        #region Конструкторы
        public Scene()
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
            fluid = new Fluid(1000, 10, 10, 100);
            speed = false;
            a = 0.0f;
            obstacle = 0;
            x_late = 0.0f;
            y_late = 0.0f;
        }
        public Scene(double gravity, double dt, int numIters, int frameNr, double overRelaxation, double obstacleX, double obstacleY, bool paused, int sceneNr, bool showPressure, bool showSmoke, Fluid fluid, bool speed, double a, int obstacle, double x_late, double y_late)
        {
            this.gravity = gravity;
            this.dt = dt;
            this.numIters = numIters;
            this.frameNr = frameNr;
            this.overRelaxation = overRelaxation;
            this.obstacleX = obstacleX;
            this.obstacleY = obstacleY;
            this.paused = paused;
            this.sceneNr = sceneNr;
            this.showPressure = showPressure;
            this.showSmoke = showSmoke;
            this.fluid = fluid;
            this.speed = speed;
            this.a = a;
            this.obstacle = obstacle;
            this.x_late = x_late;
            this.y_late = y_late;
        }
        #endregion


        
    }
}


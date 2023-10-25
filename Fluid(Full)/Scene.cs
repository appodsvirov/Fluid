using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid_Full_
{
    public class Scene
    {
        public float gravity;
        public float dt;
        public int numIters;
        public int frameNr;
        public float overRelaxation;
        public float obstacleX;
        public float obstacleY;
        public bool paused;
        public int sceneNr;
        public bool showPressure;
        public bool showSmoke;
        public Fluid fluid;
        public bool Speed;
        public float a;
        public int obstacle;
        public float x_late;
        public float y_late;

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
            fluid = null;
            Speed = false;
            a = 0.0f;
            obstacle = 0;
            x_late = 0.0f;
            y_late = 0.0f;
        }
        public Scene(float gravity, float dt, int numIters, int frameNr, float overRelaxation, float obstacleX, float obstacleY, bool paused, int sceneNr, bool showPressure, bool showSmoke, Fluid fluid, bool Speed, float a, int obstacle, float x_late, float y_late)
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
            this.Speed = Speed;
            this.a = a;
            this.obstacle = obstacle;
            this.x_late = x_late;
            this.y_late = y_late;
        }
    }
}


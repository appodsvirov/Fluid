using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FluidWPF.Models
{
    public class SolverFluid
    {
        public bool paused = false;
        public bool showPressure = false;
        public bool showSmoke = true;
        public bool Speed = false;
        public int numIters = 40; // тут ИТЕРАЦИИ!!!!
        public int frameNr = 0; 
        public int sceneNr = 0;
        public double overRelaxation = 1.9;
        public double obstacleX = 0.0;
        public double obstacleY = 0.0;
        public double gravity = -9.81;
        public double dt = 1.0 / 120.0;
        public double a = 0.0;
        public double obstacle = 0;
        public double x_late = 0.0;
        public double y_late = 0.0;

        public Fluid fluid = null;

        public double inSpeed;
        public double rad2;
        //public int countSteps;
        public int scaleNet;

        public int width = 1500;
        public int height = 900;
        double simHeight = 1;
        //double cScale = height / simHeight;
        //double simWidth = width / cScale;
        double cScale = 1500;
        double simWidth = 1500 / 900;
        int cnt = 0;

        double cX(double x) => x * cScale;
        double cY(double y) => height - y * cScale;

        public SolverFluid(double dt, double inSpeed, double rad2, int scaleNet)
        {
            //Scene.a = 0;
            //Scene.overRelaxation = 1.9;
            //Scene.numIters = 40; 

            this.dt = dt;
            this.inSpeed = inSpeed;
            this.rad2 = rad2;
            //this.countSteps = countSteps;
            this.scaleNet = scaleNet;

            int res = 100 * scaleNet;      //Размер сетки

            simWidth = 1500.0 / 900.0;

            double domainHeight = 1.0;
            double domainWidth = domainHeight / simHeight * simWidth;
            double h = domainHeight / res;

            int numX = (int)Math.Floor(domainWidth / h);
            int numY = (int)Math.Floor(domainHeight / h);

            int density = 1000;

            Fluid f = fluid = new Fluid(density, numX, numY, h);

            int n = f.numY;
            for (int i = 0; i < f.numX; i++)
            {
                for (int j = 0; j < f.numY; j++)
                {
                    double s = 1.0;    // Жижа
                    if (i == 0 || j == 0 || j == f.numY - 1)
                        s = 0.0;    // Не жижа
                    f.s[(i * n + j)] = s;


                    if (i == 1)
                    {
                        f.u[(i * n + j)] = inSpeed;
                    }
                }
            }

            double pipeH = 0.1 * f.numY;                               //Толщина струи дыма
            int minJ = (int)Math.Floor(0.5 * f.numY - 0.5 * pipeH);
            int maxJ = (int)Math.Floor(0.5 * f.numY + 0.5 * pipeH);

            for (int j = minJ; j < maxJ; j++)
            {
                f.m[j] = 0.0;
            }

            SetObstacle(0.8, 0.5, true);

            x_late = 0.8; //Скопировать сюда стартовые координаты
            y_late = 0.5;

            gravity = 0.0;
            showPressure = false;
            showSmoke = true;
        }

        public void SetObstacle(double x, double y, bool reset)
        {

            double vx = 0.0;
            double vy = 0.0;

            if (!reset)
            {
                vx = (x - obstacleX) / dt;
                vy = (y - obstacleY) / dt;
            }

            obstacleX = x;
            obstacleY = y;
            Fluid f = fluid;
            double n = f.numY;
            double cd = Math.Sqrt(2) * f.h;

            double a =  this.a;

            for (var i = 1; i < f.numX - 2; i++)
            {
                for (var j = 1; j < f.numY - 2; j++)
                {

                    f.s[(int)(i * n + j)] = 1.0;

                    double dx = (i + 0.5) * f.h - x;
                    double dy = (j + 0.5) * f.h - y;

                    double dxR = dx * Math.Cos(a) - dy * Math.Sin(a); //Вращение препятствия
                    double dyR = dx * Math.Sin(a) + dy * Math.Cos(a);

                    // ================ Окружность ==========================================================================================================================
                    //0.01
                    if (obstacle == 0 
                        && (dxR * dxR + dyR * dyR < rad2)) // rr
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Песочные часы ========================================================================================================================

                    if (obstacle == 5 && 
                        ((dxR * dxR + dyR * dyR) * (dxR * dxR + dyR * dyR)) < (0.05 * (dxR * dxR - dyR * dyR)))
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Сердце ===============================================================================================================================

                    if (obstacle == 6 &&
                        ((dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) - dxR * dxR * dyR * dyR * dyR < 0))
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Квадрат ===============================================================================================================================

                    if (obstacle == 1 &&
                        (Math.Pow(dxR, 50) + Math.Pow(dyR, 50) < Math.Pow(100, -20)))
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Профиль крыла ==========================================================================================================================

                    if ( obstacle == 2 &&

                    (((29 * (dyR + 0.02) * (dyR + 0.02) - 0.25) < dxR - 0.12 && dxR < -0.0442 && dyR > -0.029) ||

                    ((-4.5805 * dyR + 0.25) > dxR + 0.017 && dxR < 0.362 && dxR > 0.06 && dyR > -0.0281) ||

                    (((dxR - 0.0155) * (dxR - 0.015) + (dyR + 0.18123) * (dyR + 0.18123)) < 0.05 && dxR < 0.06 && dxR > -0.0442 && dyR > -0.0281) ||

                    (((dxR + 0.1118) * (dxR + 0.1118) + (dyR + 0.0196) * (dyR + 0.0196)) < 0.000342 && dxR < -0.11231 && dyR < 0.021518) ||

                    ((0.082 * (dxR) * (dxR)) < dyR + 0.039 && dxR < 0.362 && dxR > -0.11231 && dyR < -0.0281)))
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Симметричный профвиль крыла ================================================================================================================

                    if ( obstacle == 3 &&

                    (((60 * (dyR * dyR) - 0.263) < (dxR - 0.1245) && dxR < -0.0858) ||

                    ((-7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR > 0) ||

                    ((7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (-dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR > 0)) )
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Чаша ======================================================================================================================================

                    if ( obstacle == 4 && 
                        ((Math.Pow(dxR + 0.09, 2) + Math.Pow(dyR, 2) < 0.05) && (Math.Pow(dxR + 0.18, 2) + dyR * dyR > 0.06)))
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Синусоидальный тоннель =====================================================================================================================

                    if ( obstacle == 7 &&
                    (((Math.Sin(30 * dxR) > 10 * dyR + 1) && dxR < 0.3 && dxR > -0.3 && dyR > -0.3) ||

                    ((Math.Sin(30 * dxR) < 10 * dyR - 1) && dxR < 0.3 && dxR > -0.3 && dyR < 0.3)) )
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                }
            }
        }


        public void Simulate(int cnt, LogVerification logFluid)
        {
            if (!paused)
            {
                fluid.Simulate(dt, gravity, numIters, cnt, logFluid, frameNr);
                frameNr++;
            }
        }

        public void Update(int cnt, LogVerification logFluid, int i = 100)
        {
            while (frameNr < i)
            {
                if (frameNr % 100 == 0) Console.WriteLine(frameNr + " из " + i + " = " + (double)(100 * frameNr / i) + " %");
                Simulate(cnt, logFluid);
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Formats.Asn1.AsnWriter;

namespace FluidWPF.Models
{
    public class SolverFluid
    {
        public double gravity = -9.81;
        public double numIters = 100;
        public double frameNr = 0;
        public double overRelaxation = 1.9;
        public double obstacleX = 0.0;
        public double obstacleY = 0.0;
        public bool paused = false;
        public double sceneNr = 0;
        public bool showPressure = false;
        public bool showSmoke = true;
        public Fluid fluid = null;
        public bool Speed = false;
        public double a = 0.0;
        public double obstacle = 0;
        public double x_late = 0.0;
        public double y_late = 0.0;

        public double simHeight = 1.0;
        public double cScale = 900.0 / 1.0;
        double simWidth = 1500.0 / 900.0;

        public double dt;
        public double inSpeed;
        public double rad2;
        public int scaleNet;
        public SolverFluid(double dt, double inSpeed, double rad2, int scaleNet)
        {
            this.dt = dt;
            this.inSpeed = inSpeed;
            this.rad2 = rad2;
            this.scaleNet = scaleNet;
            SetupScene();
        }
        public void SetupScene(double sceneNr = 0)
        {
            this.sceneNr = sceneNr;
            overRelaxation = 1.9;

            //a = 0;


            numIters = 40;        // тут ИТЕРАЦИИ!!!!

            double res = 100 * scaleNet;      //Размер сетки

            double domainHeight = 1.0;
            double domainWidth = domainHeight / simHeight * simWidth;
            double h = domainHeight / res;

            double numX = Math.Floor(domainWidth / h);
            double numY = Math.Floor(domainHeight / h);
            double density = 1000.0;

            Fluid f = fluid = new Fluid(density, numX, numY, h);

            double n = f.numY;

            for (double i = 0; i < f.numX; i++)
            {
                for (double j = 0; j < f.numY; j++)
                {
                    double s = 1.0;    // Жижа
                    if (i == 0 || j == 0 || j == f.numY - 1)
                        s = 0.0;    // Не жижа
                    f.s[(int)(i * n + j)] = s;


                    if (i == 1)
                    {
                        f.u[(int)(i * n + j)] = inSpeed;
                    }
                }
            }

            double pipeH = 0.1 * f.numY;                               //Толщина струи дыма
            double minJ = Math.Floor(0.5 * f.numY - 0.5 * pipeH);
            double maxJ = Math.Floor(0.5 * f.numY + 0.5 * pipeH);

            for (double j = minJ; j < maxJ; j++)
            {
                f.m[(int)j] = 0.0;
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

            double a = this.a;

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
                    if ((dxR * dxR + dyR * dyR < rad2) && obstacle == 0) // rr
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Песочные часы ========================================================================================================================

                    if (((dxR * dxR + dyR * dyR) * (dxR * dxR + dyR * dyR)) < (0.05 * (dxR * dxR - dyR * dyR)) && obstacle == 5)
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Сердце ===============================================================================================================================

                    if (((dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) - dxR * dxR * dyR * dyR * dyR < 0) && obstacle == 6)
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Квадрат ===============================================================================================================================

                    if ((Math.Pow(dxR, 50) + Math.Pow(dyR, 50) < Math.Pow(100, -20)) && obstacle == 1)
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Профиль крыла ==========================================================================================================================

                    if (

                    (((29 * (dyR + 0.02) * (dyR + 0.02) - 0.25) < dxR - 0.12 && dxR < -0.0442 && dyR > -0.029) ||

                    ((-4.5805 * dyR + 0.25) > dxR + 0.017 && dxR < 0.362 && dxR > 0.06 && dyR > -0.0281) ||

                    (((dxR - 0.0155) * (dxR - 0.015) + (dyR + 0.18123) * (dyR + 0.18123)) < 0.05 && dxR < 0.06 && dxR > -0.0442 && dyR > -0.0281) ||

                    (((dxR + 0.1118) * (dxR + 0.1118) + (dyR + 0.0196) * (dyR + 0.0196)) < 0.000342 && dxR < -0.11231 && dyR < 0.021518) ||

                    ((0.082 * (dxR) * (dxR)) < dyR + 0.039 && dxR < 0.362 && dxR > -0.11231 && dyR < -0.0281)) && obstacle == 2)
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Симметричный профвиль крыла ================================================================================================================

                    if (

                    (((60 * (dyR * dyR) - 0.263) < (dxR - 0.1245) && dxR < -0.0858) ||

                    ((-7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR > 0) ||

                    ((7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (-dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR > 0)) && obstacle == 3)
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Чаша ======================================================================================================================================

                    if (((Math.Pow(dxR + 0.09, 2) + Math.Pow(dyR, 2) < 0.05) && (Math.Pow(dxR + 0.18, 2) + dyR * dyR > 0.06)) && obstacle == 4)
                    {
                        f.s[(int)(i * n + j)] = 0.0;
                        f.m[(int)(i * n + j)] = 1.0;
                        f.u[(int)(i * n + j)] = vx;
                        f.u[(int)((i + 1) * n + j)] = vx;
                        f.v[(int)(i * n + j)] = vy;
                        f.v[(int)(i * n + j) + 1] = vy;
                    }

                    // ================ Синусоидальный тоннель =====================================================================================================================

                    if (

                    (((Math.Sin(30 * dxR) > 10 * dyR + 1) && dxR < 0.3 && dxR > -0.3 && dyR > -0.3) ||

                    ((Math.Sin(30 * dxR) < 10 * dyR - 1) && dxR < 0.3 && dxR > -0.3 && dyR < 0.3)) && obstacle == 7)
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


        public void Simulate( double cnt, LogVerification logFluid)
        {
            if (!paused)
            {
                fluid.Simulate(dt, gravity, numIters,  cnt, logFluid, (int)frameNr);
                frameNr++;
            }
        }
        public void Update(double cnt, LogVerification logFluid, int i = 100)
        {
            while (frameNr < i)
            {
                if (frameNr % 100 == 0) Console.WriteLine(frameNr + " из " + i + " = " + (double)(100 * frameNr / i) + " %");
                Simulate( cnt, logFluid);
            }
            Console.WriteLine("Расчет закончен");
        }
    }


}

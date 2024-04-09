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
        public int numIters = 40;
        public int frameNr = 0;
        public double overRelaxation = 1.9;
        public double obstacleX = 0.0;
        public double obstacleY = 0.0;
        public bool paused = false;
        public bool showPressure = false;
        public bool showSmoke = true;
        public Fluid fluid = null;
        public bool Speed = false;
        public double a = 0.0;
        public int _obstacle = 0;
        public double x_late = 0.0;
        public double y_late = 0.0;


        public double canvasWidth = 1500;
        public double canvasHeight = 900;

        double simHeight = 1;
        double cScale;
        double simWidth;


        double cnt = 0;

        public double dt = 1.0 / 60.0;
        public double inSpeed = 1.0;
        public int scaleNet = 1;
        public double rad2 = 0.01;
        public bool IsSolveTurbulence;

        public SolverFluid(double dt, double inSpeed, double rad2, int scaleNet, bool isSolveTurbulence, int obstacle)
        {
            cScale = canvasHeight / simHeight;
            simWidth = canvasWidth / cScale;
            this.dt = dt;
            this.inSpeed = inSpeed;
            this.rad2 = rad2;
            this.scaleNet = scaleNet;
            IsSolveTurbulence = isSolveTurbulence;
            _obstacle = obstacle;
            SetupScene();
            
        }
        void SetupScene()
        {
            double res = 100 * scaleNet;      //Размер сетки

            double domainHeight = 1.0;
            double domainWidth = domainHeight / simHeight * simWidth;
            double h = domainHeight / res;

            int numX = (int)Math.Floor(domainWidth / h);
            int numY = (int)Math.Floor(domainHeight / h);
            int density = 1000;

            Fluid f = fluid = new Fluid(density, numX, numY, h, IsSolveTurbulence);

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

            double pipeH = 1.0 * f.numY;                               //Толщина струи дыма
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

        void SetObstacle(double x, double y, bool reset)
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
            int n = f.numY;
            double cd = Math.Sqrt(2) * f.h;

            double a = this.a;

            for (var i = 1; i < f.numX - 2; i++)
            {
                for (var j = 1; j < f.numY - 2; j++)
                {

                    f.s[(i * n + j)] = 1.0;

                    double dx = (i + 0.5) * f.h - x;
                    double dy = (j + 0.5) * f.h - y;

                    double dxR = dx * Math.Cos(a) - dy * Math.Sin(a); //Вращение препятствия
                    double dyR = dx * Math.Sin(a) + dy * Math.Cos(a);

                    // ================ Окружность ==========================================================================================================================
                    //0.01
                    if (_obstacle == 0 &&
                        (dxR * dxR + dyR * dyR < rad2)) // rr
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }
                    // ================ 4 Цилиндра ==========================================================================================================================
                    var diffRad = 0.2;
                    if (_obstacle == 44 && (
                                                ((dxR + diffRad) * (dxR + diffRad) + (dyR + diffRad) * (dyR + diffRad) < 0.01) ||
                                                ((dxR - diffRad) * (dxR - diffRad) + (dyR + diffRad) * (dyR + diffRad) < 0.01) ||
                                                ((dxR - diffRad) * (dxR - diffRad) + (dyR - diffRad) * (dyR - diffRad) < 0.01) ||
                                                ((dxR + diffRad) * (dxR + diffRad) + (dyR - diffRad) * (dyR - diffRad) < 0.01)
                                               )
                        )
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;

                    }
                    // ================ Песочные часы ========================================================================================================================

                    else if (_obstacle == 5 &&
                        ((dxR * dxR + dyR * dyR) * (dxR * dxR + dyR * dyR)) < (0.05 * (dxR * dxR - dyR * dyR)))
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }

                    // ================ Сердце ===============================================================================================================================

                    else if ( _obstacle == 6 &&
                        ((dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) - dxR * dxR * dyR * dyR * dyR < 0) )
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }

                    // ================ Квадрат ===============================================================================================================================

                    else if (_obstacle == 1 &&
                        (Math.Pow(dxR, 50) + Math.Pow(dyR, 50) < Math.Pow(100, -20)))
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }

                    // ================ Профиль крыла ==========================================================================================================================

                    else if (_obstacle == 8 &&

                    (((29 * (dyR + 0.02) * (dyR + 0.02) - 0.25) < dxR - 0.12 && dxR < -0.0442 && dyR > -0.029) ||

                    ((-4.5805 * dyR + 0.25) > dxR + 0.017 && dxR < 0.362 && dxR > 0.06 && dyR > -0.0281) ||

                    (((dxR - 0.0155) * (dxR - 0.015) + (dyR + 0.18123) * (dyR + 0.18123)) < 0.05 && dxR < 0.06 && dxR > -0.0442 && dyR > -0.0281) ||

                    (((dxR + 0.1118) * (dxR + 0.1118) + (dyR + 0.0196) * (dyR + 0.0196)) < 0.000342 && dxR < -0.11231 && dyR < 0.021518) ||

                    ((0.082 * (dxR) * (dxR)) < dyR + 0.039 && dxR < 0.362 && dxR > -0.11231 && dyR < -0.0281)))
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }

                    // ================ Симметричный профвиль крыла ================================================================================================================

                    else if (_obstacle == 3 &&

                    (((60 * (dyR * dyR) - 0.263) < (dxR - 0.1245) && dxR < -0.0858) ||

                    ((-7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR > 0) ||

                    ((7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (-dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR > 0)) )
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }

                    // ================ Чаша ======================================================================================================================================

                    else if (_obstacle == 4 && 
                        ((Math.Pow(dxR + 0.09, 2) + Math.Pow(dyR, 2) < 0.05) && (Math.Pow(dxR + 0.18, 2) + dyR * dyR > 0.06)) )
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }

                    // ================ Синусоидальный тоннель =====================================================================================================================

                    else if ( _obstacle == 7 &&

                    (((Math.Sin(30 * dxR) > 10 * dyR + 1) && dxR < 0.3 && dxR > -0.3 && dyR > -0.3) ||

                    ((Math.Sin(30 * dxR) < 10 * dyR - 1) && dxR < 0.3 && dxR > -0.3 && dyR < 0.3)))
                    {
                        f.s[(i * n + j)] = 0.0;
                        f.m[(i * n + j)] = 1.0;
                        f.u[(i * n + j)] = vx;
                        f.u[((i + 1) * n + j)] = vx;
                        f.v[(i * n + j)] = vy;
                        f.v[(i * n + j) + 1] = vy;
                    }

                }
            }
        }


        public void Simulate( double cnt, LogVerification logFluid)
        {
            if (!paused)
            {
                fluid.Simulate(dt, gravity, numIters,  cnt, logFluid, frameNr);
                frameNr++;
            }
        }
        void Update( double cnt, LogVerification logFluid, int i = 100)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fluid_Full_
{
    internal class GlobalCanvas
    {
        /// <summary>
        /// Ширина Canvas
        /// </summary>
        private int width;
        /// <summary>
        /// Высота Canvas
        /// </summary>
        private int height;
        private double simHeight;
        private double cScale;
        private double simWidth;
        private double cnt; // to do: int or double?

        public Scene scene;

        public GlobalCanvas()
        {
            width = 1500;
            height = 900;
            simHeight = 1;
            cScale = height / simHeight;
            simWidth = width / cScale;
            cnt = 0;
            scene = new Scene();
        }

        public int cX(int x)
        {
            return (int)(x * cScale); // to do: int or double?
        }
        public int cY(int y)
        {
            return (int)(height - y * cScale); // to do: int or double?
        }

        public void SetupScene(int sceneNr = 0)
        {
            scene.sceneNr = sceneNr;
            scene.overRelaxation = 1.9;

            // scene.a = 0;

            scene.dt = 1.0 / 60.0;
            scene.numIters = 40;

            int res = 100;

            double domainHeight = 1.0;
            double domainWidth = domainHeight / simHeight * simWidth;
            double h = domainHeight / res;

            int numX = (int)(domainWidth / h);
            int numY = (int)(domainHeight / h);

            double density = 1000.0;

            Fluid f = scene.fluid = new Fluid(density, numX, numY, h);

            int n = f.numY;

            double inVel = 2.0;
            for (int i = 0; i < f.numX; i++)
            {
                for (int j = 0; j < f.numY; j++)
                {
                    double s = 1.0;
                    if (i == 0 || j == 0 || j == f.numY - 1)
                        s = 0.0;
                    f.s[i * n + j] = s;

                    if (i == 1)
                    {
                        f.u[i * n + j] = inVel;
                    }
                }
            }

            double pipeH = 0.1 * f.numY;
            int minJ = (int)(0.5 * f.numY - 0.5 * pipeH);
            int maxJ = (int)(0.5 * f.numY + 0.5 * pipeH);

            for (int j = minJ; j < maxJ; j++)
                f.m[j] = 0.0;

            SetObstacle(0.8, 0.5, true);
            scene.x_late = 0.8;
            scene.y_late = 0.5;

            scene.gravity = 0.0;
            scene.showPressure = false;
            scene.showSmoke = true;
        }


        public void SetObstacle(double x, double y, bool reset)
        {
            double vx = 0.0;
            double vy = 0.0;

            if (!reset)
            {
                vx = (x - scene.obstacleX) / scene.dt;
                vy = (y - scene.obstacleY) / scene.dt;
            }



            scene.obstacleX = x;
            scene.obstacleY = y;
            var f = scene.fluid;
            var n = f.numY;
            var cd = Math.Sqrt(2) * f.h;

            var a = scene.a;

            for (int i = 1; i < f.numX - 2; i++)
            {
                for (int j = 1; j < f.numY - 2; j++)
                {
                    f.s[i * n + j] = 1.0;

                    double dx = (i + 0.5) * f.h - x;
                    double dy = (j + 0.5) * f.h - y;

                    double dxR = dx * Math.Cos(a) - dy * Math.Sin(a); //Вращение препятствия
                    double dyR = dx * Math.Sin(a) + dy * Math.Cos(a);

                    // ================ Окружность ========


                    if ((dxR * dxR + dyR * dyR < 0.01) && scene.obstacle == 0)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Песочные часы ========
                    if (((dxR * dxR + dyR * dyR) * (dxR * dxR + dyR * dyR)) < (0.05 * (dxR * dxR - dyR * dyR)) && scene.obstacle == 5)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }
                    // ================ Сердце ===============
                    if (((dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) - dxR * dxR * dyR * dyR * dyR < 0) && scene.obstacle == 6)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }
                    // ================ Квадрат ==============
                    if ((Math.Pow(dxR, 50) + Math.Pow(dyR, 50) < Math.Pow(100, -20)) && scene.obstacle == 1)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Профиль крыла =========
                    if (((29 * (dyR + 0.02) * (dyR + 0.02) - 0.25) < dxR - 0.12 && dxR < -0.0442 && dyR > -0.029) ||
                       ((-4.5805 * dyR + 0.25) > dxR + 0.017 && dxR < 0.362 && dxR > 0.06 && dyR > -0.0281) ||
                       (((dxR - 0.0155) * (dxR - 0.015) + (dyR + 0.18123) * (dyR + 0.18123)) < 0.05 && dxR < 0.06 && dxR > -0.0442 && dyR > -0.0281) ||
                       (((dxR + 0.1118) * (dxR + 0.1118) + (dyR + 0.0196) * (dyR + 0.0196)) < 0.000342 && dxR < -0.11231 && dyR < 0.021518) || ((0.082 * (dxR) * (dxR)) < dyR + 0.039 && dxR < 0.362 && dxR > -0.11231 && dyR < -0.0281) && scene.obstacle == 2)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Симметричный профиль крыла======




                    if ((((60 * (dyR * dyR) - 0.263) < (dxR - 0.1245) && dxR < -0.0858) ||
                        ((-7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR > 0) ||
                        ((7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR < 0) ||
                        (((dxR - 0.016) * (dxR - 0.016)) < (dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR < 0) ||
                        (((dxR - 0.016) * (dxR - 0.016)) < (-dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR > 0)) && scene.obstacle == 3)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Чаша ======

                    if (((Math.Pow(dxR + 0.09, 2) + Math.Pow(dyR, 2) < 0.05) && (Math.Pow(dxR + 0.18, 2) + dyR * dyR > 0.06)) && scene.obstacle == 4)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Синусоидальный тоннель ===========

                    if ((((Math.Sin(30 * dxR) > 10 * dyR + 1) && dxR < 0.3 && dxR > -0.3 && dyR > -0.3) ||
                        ((Math.Sin(30 * dxR) < 10 * dyR - 1) && dxR < 0.3 && dxR > -0.3 && dyR < 0.3)) && scene.obstacle == 7)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }
                }

            }
        }

        #region Отрисовка

        public void SetColor(int r, int g, int b)
        {
            //          c.fillStyle = `rgb(
            //	${ Math.floor(255 * r)},
            //	${ Math.floor(255 * g)},
            //	${ Math.floor(255 * b)})`
            //c.strokeStyle = `rgb(
            //	${ Math.floor(255 * r)},
            //	${ Math.floor(255 * g)},
            //	${ Math.floor(255 * b)})`

            // TO DO
        }

        public (double, double, double, double) GetSciColor(double val, int minVal, int maxVal)
        {
            val = Math.Min(Math.Max(val, minVal), maxVal - 0.0001);
            var d = maxVal - minVal;
            val = (d == 0.0) ? 0.5 : (val - minVal) / d;
            var m = 0.25;
            var num = Math.Floor(val / m);
            var s = (val - num * m) / m;
            double r = 0, g = 0, b = 0;

            switch (num)
            {
                case 0: r = 0.0; g = s; b = 1.0; break;
                case 1: r = 0.0; g = 1.0; b = 1.0 - s; break;
                case 2: r = s; g = 1.0; b = 0.0; break;
                case 3: r = 1.0; g = 1.0 - s; b = 0.0; break;
            }

            return (255 * r, 255 * g, 255 * b, 255);

        }

        public void Draw()
        {
        //    c.clearRect(0, 0, canvas.width, canvas.height);

        //    c.fillStyle = "#FF0000";
        //    f = scene.fluid;
        //    n = f.numY;

        //    var cellScale = 1;

        //    var h = f.h;

        //    minP = f.p[0];
        //    maxP = f.p[0];

        //    for (var i = 0; i < f.numCells; i++)
        //    {
        //        minP = Math.min(minP, f.p[i]);
        //        maxP = Math.max(maxP, f.p[i]);
        //    }

        //    id = c.getImageData(0, 0, canvas.width, canvas.height)


        //var color = [255, 255, 255, 255]


        //for (var i = 0; i < f.numX; i++)
        //    {
        //        for (var j = 0; j < f.numY; j++)
        //        {

        //            if (scene.showPressure)
        //            {                            // Карта давлений
        //                var p = f.p[i * n + j];
        //                var s = f.m[i * n + j];
        //                color = getSciColor(p, minP, maxP);

        //                color[2] = Math.max(0.0, color[2] - 255 * s);    // Дымовой след + Карта давлений
        //                color[1] = Math.max(0.0, color[1] - 255 * s);
        //                color[0] = Math.max(0.0, color[0] - 255 * s);
        //            }

        //            else
        //            {                                               //Чистый Дымовой
        //                var s = 1 - (f.m[i * n + j]);
        //                color[0] = 255 * s + 25;
        //                color[1] = 255 * s + 25;                             //Цвет дыма
        //                color[2] = 255 * s + 25;
        //            }

        //            var x = Math.floor(cX(i * h));
        //            var y = Math.floor(cY((j + 1) * h));
        //            var cx = Math.floor(cScale * cellScale * h) + 1;
        //            var cy = Math.floor(cScale * cellScale * h) + 1;

        //            r = color[0];
        //            g = color[1];
        //            b = color[2];

        //            for (var yi = y; yi < y + cy; yi++)
        //            {
        //                var p = 4 * (yi * canvas.width + x)
    

        //            for (var xi = 0; xi < cx; xi++)
        //                {
        //                    id.data[p++] = r;
        //                    id.data[p++] = g;
        //                    id.data[p++] = b;
        //                    id.data[p++] = 255;
        //                }
        //            }
        //        }
        //    }

        //    c.putImageData(id, 0, 0);
        }

        public void StartDrag(int x, int y)
        {
            //let bounds = canvas.getBoundingClientRect();

            //let mx = x - bounds.left - canvas.clientLeft;
            //let my = y - bounds.top - canvas.clientTop;
            //mouseDown = true;

            //x = mx / cScale;
            //y = (canvas.height - my) / cScale;

            //setObstacle(x, y, true);
        }
        public void Drag(int x, int y)
        {
            //if (mouseDown)
            //{
                //let bounds = canvas.getBoundingClientRect();
                //let mx = x - bounds.left - canvas.clientLeft;
                //let my = y - bounds.top - canvas.clientTop;
                //x = mx / cScale;
                //y = (canvas.height - my) / cScale;
                //setObstacle(x, y, false);

                //scene.x_late = x;
                //scene.y_late = y;
            //}
        }


        public void EndDrag()
        {
            //mouseDown = false;
        }
        #endregion


        #region Events
        //       canvas.addEventListener('mousedown', event => {      // Перемещение препятствия
        //           startDrag(event.x, event.y);
        //       });

        //canvas.addEventListener('mouseup', event => {
        //           endDrag();
        //       });

        //       canvas.addEventListener('mousemove', event => {
        //           drag(event.x, event.y);
        //       });

        //canvas.addEventListener('touchstart', event => {
        //           startDrag(event.touches [0].clientX, event.touches [0].clientY)
        //});

        //canvas.addEventListener('touchend', event => {
        //           endDrag()

        //   });

        //       canvas.addEventListener('touchmove', event => {

        //       event.preventDefault();
        //	event.stopImmediatePropagation();
        //	drag(event.touches [0].clientX, event.touches [0].clientY)
        //}, { passive: false});


        //document.addEventListener('keydown', event => {                         // Пауза, смена кадра
        //       switch (event.key) {

        //           case ' ': scene.paused = !scene.paused; break;
        //           case 'ArrowRight': scene.paused = false; simulate(); scene.paused = true; break;
        //           }
        //       });

        //       document.addEventListener('wheel', event => {                         // Вращение препятствия


        //           if (event.deltaY > 0){
        //               scene.a += 0.15;  //Шаг поворота
        //               setObstacle(scene.x_late, scene.y_late, true);
        //           }

        //       else
        //           {
        //               scene.a -= 0.15;  //Шаг поворота
        //               setObstacle(scene.x_late, scene.y_late, true);
        //           }
        //       });

        #endregion

        

        /// <summary>
        /// ТОТ САМЫЙ МЕТОД
        /// </summary>
        public void Simulate()
        {
            scene.fluid.Simulate(scene.dt, scene.gravity, scene.numIters);

            scene.frameNr++;
        }

        public void Update()
        {
            Simulate();
            Draw();

        }

        public void ChangeClass()
        {
            //if (scene.showPressure == true)
            //{
            //    document.getElementById('myGrid').className = "Shadow2";
            //    var button_class = document.getElementById('myGrid').className;
            //}
            //else
            //{
            //    document.getElementById('myGrid').className = "Shadow";
            //    var button_class = document.getElementById('myGrid').className;
            //}
        }
    }
}
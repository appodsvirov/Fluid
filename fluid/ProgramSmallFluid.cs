using fluid;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

class Program
{
    public static void Main(string[] args)
    {

        var canvas = new JSCanvas();
        canvas.Width = 1500;                //Размер окна
        canvas.Height = 900;

        var simHeight = 1;
        var cScale = canvas.Height / simHeight;
        var simWidth = canvas.Width / cScale;

        var cnt = 0;

        int cX(int x) => x * cScale;

        int cY(int y) => (canvas.Height - y * cScale);

        void SetupScene(int sceneNr = 0)
        {
            Scene.sceneNr = sceneNr;
            Scene.overRelaxation = 1.9;

            //Scene.a = 0;

            Scene.dt = 1.0 / 60.0;
            Scene.numIters = 40;		// тут ИТЕРАЦИИ!!!!

            var res = 100;		//Размер сетки

            var domainHeight = 1.0;
            var domainWidth = domainHeight / simHeight * simWidth;
            double h = (domainHeight / res);

            var numX = (int)Math.Floor(domainWidth / h);
            var numY = (int)Math.Floor(domainHeight / h);

            var density = 1000.0;

            var f = Scene.fluid = new Fluid(density, numX, numY, h);

            var n = f.numY;

            var inVel = 1.0;
            //var inVel = 2;
            for (var i = 0; i < f.numX; i++)
            {
                for (var j = 0; j < f.numY; j++)
                {
                    var s = 1.0;	// Жижа
                    if (i == 0 || j == 0 || j == f.numY - 1)
                    {
                        s = 0.0;    // Не жижа
                    }

                    f.s[i * n + j] = s;

                    if (i == 1)
                    {
                        f.u[i * n + j] = inVel;
                    }
                }
            }

            var pipeH = 0.1 * f.numY;								//Толщина струи дыма
            var minJ = (int)Math.Floor(0.5 * f.numY - 0.5 * pipeH);
            var maxJ = (int)Math.Floor(0.5 * f.numY + 0.5 * pipeH);

            for (int j = minJ; j < maxJ; j++)
            {
                f.m[j] = 0.0;
            }

            SetObstacle(0.8, 0.5, true);
            Scene.x_late = 0.8; //Скопировать сюда стартовые координаты
            Scene.y_late = 0.5;

            Scene.gravity = 0.0;
            Scene.showPressure = false;
            Scene.showSmoke = true;
        }

        #region Отрисовка
        //// Отрисовка

        //function setColor(r,g,b) {
        //    c.fillStyle = `rgb(
        //        ${Math.floor(255*r)},
        //        ${Math.floor(255*g)},
        //        ${Math.floor(255*b)})`
        //    c.strokeStyle = `rgb(
        //        ${Math.floor(255*r)},
        //        ${Math.floor(255*g)},
        //        ${Math.floor(255*b)})`
        //}

        //function getSciColor(val, minVal, maxVal) {
        //    val = Math.min(Math.max(val, minVal), maxVal- 0.0001);
        //    var d = maxVal - minVal;
        //    val = d == 0.0 ? 0.5 : (val - minVal) / d;
        //    var m = 0.25;
        //    var num = Math.floor(val / m);
        //    var s = (val - num * m) / m;
        //    var r, g, b;

        //    switch (num) {
        //        case 0 : r = 0.0;    g = s;         b = 1.0; break;
        //        case 1 : r = 0.0;    g = 1.0;       b = 1.0 - s; break;
        //        case 2 : r = s;      g = 1.0;       b = 0.0; break;
        //        case 3 : r = 1.0;    g = 1.0 - s;   b = 0.0; break;
        //    }

        //    return[255*r,255*g,255*b, 255]
        //}

        //function draw() 
        //{
        //    c.clearRect(0, 0, canvas.width, canvas.height);

        //    c.fillStyle = "#FF0000";
        //    f = Scene.fluid;
        //    n = f.numY;

        //    var cellScale = 1;

        //    var h = f.h;

        //    minP = f.p[0];
        //    maxP = f.p[0];

        //    for (var i = 0; i < f.numCells; i++) {
        //        minP = Math.min(minP, f.p[i]);
        //        maxP = Math.max(maxP, f.p[i]);
        //    }

        //    id = c.getImageData(0,0, canvas.width, canvas.height)

        //    var color = [255, 255, 255, 255]

        //    for (var i = 0; i < f.numX; i++) {
        //        for (var j = 0; j < f.numY; j++) {

        //            if (Scene.showPressure) {                            // Карта давлений
        //                var p = f.p[i*n + j];
        //                var s = f.m[i*n + j];
        //                color = getSciColor(p, minP, maxP);

        //                color[2] = Math.max(0.0, color[2] - 255 * s);    // Дымовой след + Карта давлений
        //                color[1] = Math.max(0.0, color[1] - 255 * s);
        //                color[0] = Math.max(0.0, color[0] - 255 * s);					
        //            }

        //            else {                                               //Чистый Дымовой
        //                var s = 1-(f.m[i*n + j]);
        //                color[0] = 255*s + 25;
        //                color[1] = 43*s + 25;							 //Цвет дыма
        //                color[2] = 43*s + 25;

        //                if(i === 99 && j === 38)
        //                {
        //                    color[0] = 0*s + 25;
        //                    color[1] = 0*s + 25;							
        //                    color[2] = 255;
        //                }
        //                if(i === 99 && j === 62)
        //                {
        //                    color[0] = 0*s + 25;
        //                    color[1] = 0*s + 25;							
        //                    color[2] = 255;
        //                }
        //            }

        //            var x = Math.floor(cX(i * h));
        //            var y = Math.floor(cY((j+1) * h));
        //            var cx = Math.floor(cScale * cellScale * h) + 1;
        //            var cy = Math.floor(cScale * cellScale * h) + 1;

        //            r = color[0];
        //            g = color[1];
        //            b = color[2];

        //            for (var yi = y; yi < y + cy; yi++) {
        //                var p = 4 * (yi * canvas.width + x)

        //                for (var xi = 0; xi < cx; xi++) {
        //                    id.data[p++] = r;
        //                    id.data[p++] = g;
        //                    id.data[p++] = b;
        //                    id.data[p++] = 255;
        //                }


        //            }

        //        }
        //    }

        //    c.putImageData(id, 0, 0);
        //}
        #endregion



        void SetObstacle(double x, double y, bool reset)
        {

            var vx = 0.0;
            var vy = 0.0;

            if (!reset)
            {
                vx = (x - Scene.obstacleX) / Scene.dt;
                vy = (y - Scene.obstacleY) / Scene.dt;
            }

            Scene.obstacleX = x;
            Scene.obstacleY = y;
            var f = Scene.fluid;
            var n = f.numY;
            var cd = Math.Sqrt(2) * f.h;

            var a = Scene.a;

            double dx, dy, dxR, dyR;

            for (var i = 1; i < f.numX - 2; i++)
            {
                for (var j = 1; j < f.numY - 2; j++)
                {

                    f.s[i * n + j] = 1.0;

                    dx = (i + 0.5) * f.h - x;
                    dy = (j + 0.5) * f.h - y;

                    dxR = dx * Math.Cos(a) - dy * Math.Sin(a); //Вращение препятствия
                    dyR = dx * Math.Sin(a) + dy * Math.Cos(a);

                    // ================ Окружность ==========================================================================================================================

                    if ((dxR * dxR + dyR * dyR < 0.01) && Scene.obstacle == 0)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Песочные часы ========================================================================================================================

                    if (((dxR * dxR + dyR * dyR) * (dxR * dxR + dyR * dyR)) < (0.05 * (dxR * dxR - dyR * dyR)) && Scene.obstacle == 5)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Сердце ===============================================================================================================================

                    if (((dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) - dxR * dxR * dyR * dyR * dyR < 0) && Scene.obstacle == 6)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Квадрат ===============================================================================================================================

                    if ((Math.Pow(dxR, 50) + Math.Pow(dyR, 50) < Math.Pow(100, -20)) && Scene.obstacle == 1)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Профиль крыла ==========================================================================================================================

                    if (

                    (((29 * (dyR + 0.02) * (dyR + 0.02) - 0.25) < dxR - 0.12 && dxR < -0.0442 && dyR > -0.029) ||

                    ((-4.5805 * dyR + 0.25) > dxR + 0.017 && dxR < 0.362 && dxR > 0.06 && dyR > -0.0281) ||

                    (((dxR - 0.0155) * (dxR - 0.015) + (dyR + 0.18123) * (dyR + 0.18123)) < 0.05 && dxR < 0.06 && dxR > -0.0442 && dyR > -0.0281) ||

                    (((dxR + 0.1118) * (dxR + 0.1118) + (dyR + 0.0196) * (dyR + 0.0196)) < 0.000342 && dxR < -0.11231 && dyR < 0.021518) ||

                    ((0.082 * (dxR) * (dxR)) < dyR + 0.039 && dxR < 0.362 && dxR > -0.11231 && dyR < -0.0281)) && Scene.obstacle == 2)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Симметричный профвиль крыла ================================================================================================================

                    if (

                    (((60 * (dyR * dyR) - 0.263) < (dxR - 0.1245) && dxR < -0.0858) ||

                    ((-7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR > 0) ||

                    ((7.5 * dyR) > (dxR - 0.35) && dxR < 0.35 && dxR > 0.0783 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR < 0) ||

                    (((dxR - 0.016) * (dxR - 0.016)) < (-dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR > 0)) && Scene.obstacle == 3)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Чаша ======================================================================================================================================

                    if (((Math.Pow(dxR + 0.09, 2) + Math.Pow(dyR, 2) < 0.05) && (Math.Pow(dxR + 0.18, 2) + dyR * dyR > 0.06)) && Scene.obstacle == 4)
                    {
                        f.s[i * n + j] = 0.0;
                        f.m[i * n + j] = 1.0;
                        f.u[i * n + j] = vx;
                        f.u[(i + 1) * n + j] = vx;
                        f.v[i * n + j] = vy;
                        f.v[i * n + j + 1] = vy;
                    }

                    // ================ Синусоидальный тоннель =====================================================================================================================

                    if (

                    (((Math.Sin(30 * dxR) > 10 * dyR + 1) && dxR < 0.3 && dxR > -0.3 && dyR > -0.3) ||

                    ((Math.Sin(30 * dxR) < 10 * dyR - 1) && dxR < 0.3 && dxR > -0.3 && dyR < 0.3)) && Scene.obstacle == 7)
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

        #region Перемещение препятствия

        //var mouseDown = false;

        //function startDrag(x, y) {
        //    let bounds = canvas.getBoundingClientRect();

        //    let mx = x - bounds.left - canvas.clientLeft;
        //    let my = y - bounds.top - canvas.clientTop;
        //    mouseDown = true;

        //    x = mx / cScale;
        //    y = (canvas.height - my) / cScale;

        //    setObstacle(x,y, true);
        //}

        //function drag(x, y) {
        //    if (mouseDown) {
        //        let bounds = canvas.getBoundingClientRect();
        //        let mx = x - bounds.left - canvas.clientLeft;
        //        let my = y - bounds.top - canvas.clientTop;
        //        x = mx / cScale;
        //        y = (canvas.height - my) / cScale;
        //        setObstacle(x,y, false);

        //        Scene.x_late = x;
        //        Scene.y_late = y;
        //    }
        //}

        //function endDrag() {
        //    mouseDown = false;
        //}

        //canvas.addEventListener('mousedown', event => {      // Перемещение препятствия
        //    startDrag(event.x, event.y);
        //});

        //canvas.addEventListener('mouseup', event => {
        //    endDrag();
        //});

        //canvas.addEventListener('mousemove', event => {
        //    drag(event.x, event.y);
        //});

        //canvas.addEventListener('touchstart', event => {
        //    startDrag(event.touches[0].clientX, event.touches[0].clientY)
        //});

        //canvas.addEventListener('touchend', event => {
        //    endDrag()
        //});

        //canvas.addEventListener('touchmove', event => {
        //    event.preventDefault();
        //    event.stopImmediatePropagation();
        //    drag(event.touches[0].clientX, event.touches[0].clientY)
        //}, { passive: false});


        //document.addEventListener('keydown', event => {							// Пауза, смена кадра
        //    switch(event.key) {
        //        case ' ': Scene.paused = !Scene.paused; break;
        //        case 'ArrowRight': Scene.paused = false; simulate(); Scene.paused = true; break;
        //    }
        //});

        //document.addEventListener('wheel', event => {	                      // Вращение препятствия


        //    if(event.deltaY>0){
        //        Scene.a += 0.15;  //Шаг поворота
        //        setObstacle(Scene.x_late , Scene.y_late, true);
        //    }
        //    else{
        //        Scene.a -= 0.15;  //Шаг поворота
        //        setObstacle(Scene.x_late , Scene.y_late, true);
        //    }	
        //});
        #endregion

        void Simulate(LogFluid lf)
        {
            if (!Scene.paused)
                Scene.fluid.Simulate(Scene.dt, Scene.gravity, Scene.numIters, lf);
            Scene.frameNr++;
        }

        void Update(int i = 100)
        {
            //Simulate();
            //Draw();
            //RequestAnimationFrame(update);
            LogFluid lf = new LogFluid();
            while (Scene.frameNr < i)
            {
                if (Scene.frameNr % 10 == 0) Console.WriteLine(Scene.frameNr + " из " + i);
                Simulate(lf);
            }
            lf.GetLog();

        }

        void ChangeClass()
        {
            //if(Scene.showPressure == true){
            //    document.getElementById('myGrid').className = "Shadow2";
            //    var button_class = document.getElementById('myGrid').className;
            //}   
            //else
            //{
            //    document.getElementById('myGrid').className = "Shadow";
            //    var button_class = document.getElementById('myGrid').className;
            //}
        }

        SetupScene(1);
        Update(602);

    }
}

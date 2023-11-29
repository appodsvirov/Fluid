using FluidV3;
using static System.Runtime.InteropServices.JavaScript.JSType;

var canvas = new Canvas();
canvas.width = 1500;
canvas.height = 900;

double simHeight = 1;
double cScale = canvas.height / simHeight;
double simWidth = canvas.width / cScale;


double cnt = 0;

double cX(double x) => x * cScale;
double cY(double y) => canvas.height - y * cScale;

//LogFluid logFluid = new LogFluid(); 

//SetupScene(1);
//Update(ref cnt, logFluid, GlobalValues.steps);

//logFluid.GetLog();

using (StreamReader streamReader = new StreamReader("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\P_Up.txt"))
{
    using (StreamWriter streamWriter = new StreamWriter("C:\\Users\\USER\\Desktop\\fluid\\FluidV3\\output\\P_500.txt"))
    {
        int tmp = 0;
        while (!streamReader.EndOfStream)
        {
            if (tmp % 500 == 0)
                streamWriter.WriteLine(streamReader.ReadLine());
            else
                streamReader.ReadLine();
            tmp++;
        }
    }
}

    void SetupScene(double sceneNr = 0)
    {
        Scene.sceneNr = sceneNr;
        Scene.overRelaxation = 1.9;

        //Scene.a = 0;

        Scene.dt = 1.0 / 60.0;
        Scene.numIters = 40;        // тут ИТЕРАЦИИ!!!!

        double res = 100;      //Размер сетки

        double domainHeight = 1.0;
        double domainWidth = domainHeight / simHeight * simWidth;
        double h = domainHeight / res;

        double numX = Math.Floor(domainWidth / h);
        double numY = Math.Floor(domainHeight / h);

        double density = 1000.0;

        Fluid f = Scene.fluid = new Fluid(density, numX, numY, h);

        double n = f.numY;

        double inVel = GlobalValues.inVel; //ww


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
                    f.u[(int)(i * n + j)] = inVel;
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

        Scene.x_late = 0.8; //Скопировать сюда стартовые координаты
        Scene.y_late = 0.5;

        Scene.gravity = 0.0;
        Scene.showPressure = false;
        Scene.showSmoke = true;
    }

void SetObstacle(double x, double y, bool reset)
{

    double vx = 0.0;
    double vy = 0.0;

    if (!reset)
    {
        vx = (x - Scene.obstacleX) / Scene.dt;
        vy = (y - Scene.obstacleY) / Scene.dt;
    }

    Scene.obstacleX = x;
    Scene.obstacleY = y;
    Fluid f = Scene.fluid;
    double n = f.numY;
    double cd = Math.Sqrt(2) * f.h;

    double a = Scene.a;

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
            if ((dxR * dxR + dyR * dyR < GlobalValues.rad2) && Scene.obstacle == 0) // rr
            {
                f.s[(int)(i * n + j)] = 0.0;
                f.m[(int)(i * n + j)] = 1.0;
                f.u[(int)(i * n + j)] = vx;
                f.u[(int)((i + 1) * n + j)] = vx;
                f.v[(int)(i * n + j)] = vy;
                f.v[(int)(i * n + j) + 1] = vy;
            }

            // ================ Песочные часы ========================================================================================================================

            if (((dxR * dxR + dyR * dyR) * (dxR * dxR + dyR * dyR)) < (0.05 * (dxR * dxR - dyR * dyR)) && Scene.obstacle == 5)
            {
                f.s[(int)(i * n + j)] = 0.0;
                f.m[(int)(i * n + j)] = 1.0;
                f.u[(int)(i * n + j)] = vx;
                f.u[(int)((i + 1) * n + j)] = vx;
                f.v[(int)(i * n + j)] = vy;
                f.v[(int)(i * n + j) + 1] = vy;
            }

            // ================ Сердце ===============================================================================================================================

            if (((dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) * (dxR * dxR + dyR * dyR - 0.01) - dxR * dxR * dyR * dyR * dyR < 0) && Scene.obstacle == 6)
            {
                f.s[(int)(i * n + j)] = 0.0;
                f.m[(int)(i * n + j)] = 1.0;
                f.u[(int)(i * n + j)] = vx;
                f.u[(int)((i + 1) * n + j)] = vx;
                f.v[(int)(i * n + j)] = vy;
                f.v[(int)(i * n + j) + 1] = vy;
            }

            // ================ Квадрат ===============================================================================================================================

            if ((Math.Pow(dxR, 50) + Math.Pow(dyR, 50) < Math.Pow(100, -20)) && Scene.obstacle == 1)
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

            ((0.082 * (dxR) * (dxR)) < dyR + 0.039 && dxR < 0.362 && dxR > -0.11231 && dyR < -0.0281)) && Scene.obstacle == 2)
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

            (((dxR - 0.016) * (dxR - 0.016)) < (-dyR + 0.0401) && dxR < 0.0793 && dxR > -0.0858 && dyR > 0)) && Scene.obstacle == 3)
            {
                f.s[(int)(i * n + j)] = 0.0;
                f.m[(int)(i * n + j)] = 1.0;
                f.u[(int)(i * n + j)] = vx;
                f.u[(int)((i + 1) * n + j)] = vx;
                f.v[(int)(i * n + j)] = vy;
                f.v[(int)(i * n + j) + 1] = vy;
            }

            // ================ Чаша ======================================================================================================================================

            if (((Math.Pow(dxR + 0.09, 2) + Math.Pow(dyR, 2) < 0.05) && (Math.Pow(dxR + 0.18, 2) + dyR * dyR > 0.06)) && Scene.obstacle == 4)
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

            ((Math.Sin(30 * dxR) < 10 * dyR - 1) && dxR < 0.3 && dxR > -0.3 && dyR < 0.3)) && Scene.obstacle == 7)
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


void Simulate(ref double cnt, LogFluid logFluid)
{
    if (!Scene.paused)
    {
        Scene.fluid.Simulate(Scene.dt, Scene.gravity, Scene.numIters, ref cnt, logFluid);
        Scene.frameNr++;
    }
}

void Update(ref double cnt, LogFluid logFluid, int i = 100)
{
    while (Scene.frameNr < i)
    {
        if (Scene.frameNr % 100 == 0) Console.WriteLine(Scene.frameNr + " из " + i);
        Simulate(ref cnt, logFluid);
    }

    Console.WriteLine("Расчет закончен");
}

class Canvas
{
    public double width;
    public double height;
}

public class Fluid
{
    public double density;
    public double numX;
    public double numY;
    public double numCells;
    public double h;
    public double[] u;
    public double[] v;
    public double[] newU;
    public double[] newV;
    public double[] p;
    public double[] s;
    public double[] m;
    public double[] newM;
    public double num;
    public Fluid(double density, double numX, double numY, double h)
    {
        this.density = density;
        this.numX = numX + 2;
        this.numY = numY + 2;
        this.numCells = this.numX * this.numY;
        this.h = h;
        this.u = new double[(int)this.numCells];
        this.v = new double[(int)this.numCells];
        this.newU = new double[(int)this.numCells];
        this.newV = new double[(int)this.numCells];
        this.p = new double[(int)this.numCells];
        this.s = new double[(int)this.numCells];
        this.newM = new double[(int)this.numCells];
        this.m = Enumerable.Repeat(1.0, (int)this.numCells).ToArray();


        num = numX * numY;
    }

    public void Integrate(double dt, double gravity) // Гравитация
    {
        var n = this.numY;
        for (var i = 1; i < this.numX; i++)
        {
            for (var j = 1; j < this.numY - 1; j++)
            {
                if (this.s[(int)(i * n + j)] != 0.0 && this.s[(int)(i * n + j - 1)] != 0.0)
                {
                    this.v[(int)(i * n + j)] += gravity * dt;
                }
            }
        }
    }


    public void SolveIncompressibility(double numIters, double dt) // Несжимаемость
    {

        double n = this.numY;
        double cp = this.density * this.h / dt;
        //double cp = 1000;
        for (var iter = 0; iter < numIters; iter++)
        {

            for (var i = 1; i < this.numX - 1; i++)
            {
                for (var j = 1; j < this.numY - 1; j++)
                {

                    if (this.s[(int)(i * n + j)] == 0.0)
                    {
                        continue;
                    }

                    var s = this.s[(int)(i * n + j)];
                    var sx0 = this.s[(int)((i - 1) * n + j)];
                    var sx1 = this.s[(int)((i + 1) * n + j)];
                    var sy0 = this.s[(int)(i * n + j - 1)];
                    var sy1 = this.s[(int)(i * n + j + 1)];
                    s = sx0 + sx1 + sy0 + sy1;
                    if (s == 0.0)
                    {
                        continue;
                    }

                    var div = this.u[(int)((i + 1) * n + j)] - this.u[(int)(i * n + j)] +
                        this.v[(int)(i * n + j) + 1] - this.v[(int)(i * n + j)];

                    var p = -div / s;
                    p *= Scene.overRelaxation;


                    this.p[(int)(i * n + j)] += cp * p;

                    this.u[(int)(i * n + j)] -= sx0 * p;
                    this.u[(int)((i + 1) * n + j)] += sx1 * p;
                    this.v[(int)(i * n + j)] -= sy0 * p;
                    this.v[(int)(i * n + j) + 1] += sy1 * p;
                }
            }
        }
    }


    public void Extrapolate() // Экстраполяция
    {
        double n = this.numY;
        for (var i = 0; i < this.numX; i++)
        {
            this.u[(int)(i * n + 0)] = this.u[(int)(i * n + 1)];
            this.u[(int)(i * n + this.numY - 1)] = this.u[(int)(i * n + this.numY - 2)];
        }
        for (var j = 0; j < this.numY; j++)
        {
            this.v[(int)(0 * n + j)] = this.v[(int)(1 * n + j)];
            this.v[(int)((this.numX - 1) * n + j)] = this.v[(int)((this.numX - 2) * n + j)];

        }
    }

    public double SampleField(double x, double y, SearchValue field)
    {
        double n = this.numY;
        double h = this.h;
        double h1 = 1.0 / h;
        double h2 = 0.5 * h;

        x = Math.Max(Math.Min(x, this.numX * h), h);
        y = Math.Max(Math.Min(y, this.numY * h), h);

        double dx = 0.0;
        double dy = 0.0;

        double dxR = 0.0;
        double dyR = 0.0;

        double a = 0.0;

        double[] f = new double[0];

        switch (field)
        {
            case SearchValue.U_FIELD: f = this.u; dy = h2; break;
            case SearchValue.V_FIELD: f = this.v; dx = h2; break;
            case SearchValue.S_FIELD: f = this.m; dx = h2; dy = h2; break;
        }

        var x0 = Math.Min(Math.Floor((x - dx) * h1), this.numX - 1);
        var tx = ((x - dx) - x0 * h) * h1;
        var x1 = Math.Min(x0 + 1, this.numX - 1);

        var y0 = Math.Min(Math.Floor((y - dy) * h1), this.numY - 1);
        var ty = ((y - dy) - y0 * h) * h1;
        var y1 = Math.Min(y0 + 1, this.numY - 1);

        var sx = 1.0 - tx;
        var sy = 1.0 - ty;

        var val = sx * sy * f[(int)(x0 * n + y0)] +
            tx * sy * f[(int)(x1 * n + y0)] +
            tx * ty * f[(int)(x1 * n + y1)] +
            sx * ty * f[(int)(x0 * n + y1)];

        return val;
    }

    public double AvgU(double i, double j)
    {
        var n = this.numY;
        var u = (this.u[(int)(i * n + j - 1)] + this.u[(int)(i * n + j)] +
            this.u[(int)((i + 1) * n + j - 1)] + this.u[(int)((i + 1) * n + j)]) * 0.25;
        return u;
    }

    public double AvgV(double i, double j)
    {
        var n = this.numY;
        var v = (this.v[(int)((i - 1) * n + j)] + this.v[(int)(i * n + j)] +
            this.v[(int)((i - 1) * n + j) + 1] + this.v[(int)(i * n + j) + 1]) * 0.25;
        return v;
    }

    public void AdvectVel(double dt, ref double cnt) // Адвекция
    {

        Array.Copy(this.u, newU, this.u.Length); //this.newU.set(this.u); 
        Array.Copy(this.v, newV, this.v.Length); //this.newV.set(this.v);

        double n = this.numY;
        double h = this.h;
        double h2 = 0.5 * h;

        for (int i = 1; i < this.numX; i++)
        {
            for (int j = 1; j < this.numY; j++)
            {

                cnt++;

                // Горизонтальная составляющая (u)
                if (this.s[(int)(i * n + j)] != 0.0 && this.s[(int)((i - 1) * n + j)] != 0.0 && j < this.numY - 1)
                {
                    var x = i * h;
                    var y = j * h + h2;
                    var u = this.u[(int)(i * n + j)];
                    var v = this.AvgV(i, j);
                    x = x - dt * u;
                    y = y - dt * v;
                    u = this.SampleField(x, y, SearchValue.U_FIELD);
                    this.newU[(int)(i * n + j)] = u;
                }
                // Вертикальная составляющая (v)
                if (this.s[(int)(i * n + j)] != 0.0 && this.s[(int)(i * n + j) - 1] != 0.0 && i < this.numX - 1)
                {
                    var x = i * h + h2;
                    var y = j * h;
                    var u = this.AvgU(i, j);
                    var v = this.v[(int)(i * n + j)];
                    x = x - dt * u;
                    y = y - dt * v;
                    v = this.SampleField(x, y, SearchValue.V_FIELD);
                    this.newV[(int)(i * n + j)] = v;
                }
            }
        }

        Array.Copy(newU, this.u, newU.Length); //this.u.set(this.newU);
        Array.Copy(newV, this.v, newV.Length); // this.v.set(this.newV);
    }

    public void AdvectSmoke(double dt)
    {
        Array.Copy(this.m, this.newM, this.m.Length); //  this.newM.set(this.m);


        var n = this.numY;
        var h = this.h;
        var h2 = 0.5 * h;

        for (var i = 1; i < this.numX - 1; i++)
        {
            for (var j = 1; j < this.numY - 1; j++)
            {

                if (this.s[(int)(i * n + j)] != 0.0)
                {

                    double u, v;
                    if (Scene.Speed) // Переключение числа Рейнольдса
                    {
                        u = (this.u[(int)(i * n + j)] + this.u[(int)((i + 1) * n + j)]) * 0.03;   // Скорость
                        v = (this.v[(int)(i * n + j)] + this.v[(int)(i * n + j + 1)]) * 0.03;
                    }
                    else
                    {
                        u = (this.u[(int)(i * n + j)] + this.u[(int)((i + 1) * n + j)]) * 0.5;    // Скорость
                        v = (this.v[(int)(i * n + j)] + this.v[(int)(i * n + j + 1)]) * 0.5;
                    }

                    double x = i * h + h2 - dt * u;
                    double y = j * h + h2 - dt * v;    // Направление

                    this.newM[(int)(i * n + j)] = this.SampleField(x, y, SearchValue.S_FIELD);
                }
            }
        }
        Array.Copy(this.newM, this.m, this.newM.Length); //this.m.set(this.newM);
    }

    public void Simulate(double dt, double gravity, double numIters, ref double cnt, LogFluid lf)
    {

        this.Integrate(dt, gravity);

        this.p = Enumerable.Repeat(0.0, (int)(this.numCells)).ToArray();//this.p.fill(0.0);
        this.SolveIncompressibility(numIters, dt);

        this.Extrapolate();
        this.AdvectVel(dt, ref cnt);
        this.AdvectSmoke(dt);

        if (lf != null)
        {
            double n = numY;
            lf.Log(p[(int)(lf.pointUp.Item1 * numY + lf.pointUp.Item2)],
                p[(int)(lf.pointDown.Item1 * numY + lf.pointDown.Item2)],
                m[(int)(lf.pointUp.Item1 * numY + lf.pointUp.Item2)],
                m[(int)(lf.pointDown.Item1 * numY + lf.pointDown.Item2)],
                Math.Sqrt(Math.Pow(u[(int)(lf.pointUp.Item1 * numY + lf.pointUp.Item2)], 2) *
                Math.Pow(v[(int)(lf.pointUp.Item1 * numY + lf.pointUp.Item2)], 2)),
                Math.Sqrt(Math.Pow(u[(int)(lf.pointDown.Item1 * numY + lf.pointDown.Item2)], 2) * 
                Math.Pow(v[(int)(lf.pointDown.Item1 * numY + lf.pointDown.Item2)], 2))
                );
        }
    }
}

static class GlobalValues //gg
{
    public static double inVel = 0.006;
    public static double rad2 = 0.0025;
    public static int steps = 200002;
}
static class Scene
{
    static public double gravity = -9.81;
    static public double dt = 1.0 / 120.0;
    static public double numIters = 100;
    static public double frameNr = 0;
    static public double overRelaxation = 1.9;
    static public double obstacleX = 0.0;
    static public double obstacleY = 0.0;
    static public bool paused = false;
    static public double sceneNr = 0;
    static public bool showPressure = false;
    static public bool showSmoke = true;
    static public Fluid fluid = null;
    static public bool Speed = false;
    static public double a = 0.0;
    static public double obstacle = 0;
    static public double x_late = 0.0;
    static public double y_late = 0.0;
}


public enum SearchValue
{
    U_FIELD = 0,
    V_FIELD = 1,
    S_FIELD = 2
}

/*
	// Отрисовка

	function setColor(r,g,b) {
		c.fillStyle = `rgb(
			${Math.floor(255*r)},
			${Math.floor(255*g)},
			${Math.floor(255*b)})`
		c.strokeStyle = `rgb(
			${Math.floor(255*r)},
			${Math.floor(255*g)},
			${Math.floor(255*b)})`
	}

	function getSciColor(val, minVal, maxVal) {
		val = Math.min(Math.max(val, minVal), maxVal- 0.0001);
		var d = maxVal - minVal;
		val = d == 0.0 ? 0.5 : (val - minVal) / d;
		var m = 0.25;
		var num = Math.floor(val / m);
		var s = (val - num * m) / m;
		var r, g, b;

		switch (num) {
			case 0 : r = 0.0;    g = s;         b = 1.0; break;
			case 1 : r = 0.0;    g = 1.0;       b = 1.0 - s; break;
			case 2 : r = s;      g = 1.0;       b = 0.0; break;
			case 3 : r = 1.0;    g = 1.0 - s;   b = 0.0; break;
		}

		return[255*r,255*g,255*b, 255]
	}

	function draw() 
	{
		c.clearRect(0, 0, canvas.width, canvas.height);

		c.fillStyle = "#FF0000";
		f = scene.fluid;
		n = f.numY;

		var cellScale = 1;
		
		var h = f.h;

		minP = f.p[0];
		maxP = f.p[0];

		for (var i = 0; i < f.numCells; i++) {
			minP = Math.min(minP, f.p[i]);
			maxP = Math.max(maxP, f.p[i]);
		}

		id = c.getImageData(0,0, canvas.width, canvas.height)

		var color = [255, 255, 255, 255]

		for (var i = 0; i < f.numX; i++) {
			for (var j = 0; j < f.numY; j++) {

				if (scene.showPressure) {                            // Карта давлений
					var p = f.p[i*n + j];
					var s = f.m[i*n + j];
					color = getSciColor(p, minP, maxP);

					color[2] = Math.max(0.0, color[2] - 255 * s);    // Дымовой след + Карта давлений
					color[1] = Math.max(0.0, color[1] - 255 * s);
					color[0] = Math.max(0.0, color[0] - 255 * s);					
				}

				else {                                               //Чистый Дымовой
					var s = 1-(f.m[i*n + j]);
					//var s = Math.sqrt(f.u[i*n + j] * f.u[i*n + j] + f.v[i*n + j] * f.v[i*n + j])/2;
					color[0] = 255*s;
					color[1] = 43*s;							 //Цвет дыма
					color[2] = 43*s;
					if(scene.frameNr > 302)
					{
						color[1] = 255*s;							 //Цвет дыма
						color[2] = 255*s;
					}
					if(i === 99 && j === 38) // нижняя точка
					{
						color[0] = 0*s + 25;
						color[1] = 0*s + 25;							
						color[2] = 255;
					}
					if(i === 99 && j === 62) // верхняя точка
					{
						color[0] = 0*s + 25;
						color[1] = 0*s + 25;							
						color[2] = 255;
					}
				}

				var x = Math.floor(cX(i * h));
				var y = Math.floor(cY((j+1) * h));
				var cx = Math.floor(cScale * cellScale * h) + 1;
				var cy = Math.floor(cScale * cellScale * h) + 1;

				r = color[0];
				g = color[1];
				b = color[2];

				for (var yi = y; yi < y + cy; yi++) {
					var p = 4 * (yi * canvas.width + x)

					for (var xi = 0; xi < cx; xi++) {
						id.data[p++] = r;
						id.data[p++] = g;
						id.data[p++] = b;
						id.data[p++] = 255;
					}
					
					
				}
				
			}
		}

		c.putImageData(id, 0, 0);
	}

	

	// Перемещение препятствия

	var mouseDown = false;

	function startDrag(x, y) {
		let bounds = canvas.getBoundingClientRect();

		let mx = x - bounds.left - canvas.clientLeft;
		let my = y - bounds.top - canvas.clientTop;
		mouseDown = true;

		x = mx / cScale;
		y = (canvas.height - my) / cScale;

		setObstacle(x,y, true);
	}

	function drag(x, y) {
		if (mouseDown) {
			let bounds = canvas.getBoundingClientRect();
			let mx = x - bounds.left - canvas.clientLeft;
			let my = y - bounds.top - canvas.clientTop;
			x = mx / cScale;
			y = (canvas.height - my) / cScale;
			setObstacle(x,y, false);

			scene.x_late = x;
			scene.y_late = y;
		}
	}

	function endDrag() {
		mouseDown = false;
	}

	canvas.addEventListener('mousedown', event => {      // Перемещение препятствия
		startDrag(event.x, event.y);
	});

	canvas.addEventListener('mouseup', event => {
		endDrag();
	});

	canvas.addEventListener('mousemove', event => {
		drag(event.x, event.y);
	});

	canvas.addEventListener('touchstart', event => {
		startDrag(event.touches[0].clientX, event.touches[0].clientY)
	});

	canvas.addEventListener('touchend', event => {
		endDrag()
	});

	canvas.addEventListener('touchmove', event => {
		event.preventDefault();
		event.stopImmediatePropagation();
		drag(event.touches[0].clientX, event.touches[0].clientY)
	}, { passive: false});


	document.addEventListener('keydown', event => {							// Пауза, смена кадра
		switch(event.key) {
			case ' ': scene.paused = !scene.paused; break;
			case 'ArrowRight': scene.paused = false; simulate(); scene.paused = true; break;
		}
	});

	document.addEventListener('wheel', event => {	                      // Вращение препятствия
		

		if(event.deltaY>0){
			scene.a += 0.15;  //Шаг поворота
			setObstacle(scene.x_late , scene.y_late, true);
		}
		else{
			scene.a -= 0.15;  //Шаг поворота
			setObstacle(scene.x_late , scene.y_late, true);
		}	
	});




	function changeClass()
	{
		if(scene.showPressure == true){
			document.getElementById('myGrid').className = "Shadow2";
			var button_class = document.getElementById('myGrid').className;
		}   
		else
		{
			document.getElementById('myGrid').className = "Shadow";
			var button_class = document.getElementById('myGrid').className;
		}
	}

	setupScene(1);
	update();
*/
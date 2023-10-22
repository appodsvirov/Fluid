using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

class Fluid
{
    private double density; //density: плотность жидкости
    private int numX; //numX : количество ячеек по оси X 
    private int numY; //numY: количество ячеек по оси Y
    private int numCells; // numCells: общее количество ячеек в системе
    private int h; // h: размер ячейки
    private List<double> u; // u: скорости в направлении X
    private List<double> v; // v: скорости в направлении Y
    private List<double> newU; // newU: новое значение скорости в направлении X
    private List<double> newV; // newV: новое значение скорости в направлении Y
    private List<double> p; //p: давление
    private List<double> s; //s: массовый источник
    private List<double> m; //m: массовые коэффициенты
    private List<double> newM;  // newM: новые массовые коэффициенты
    private int num; //num: общее количество ячеек 

    // из обьекта scene
    public const double gravity = 9.81; //gravity: ускорение свободного падения
    public const double dt = 1.0 / 120.0; //dt: шаг по времени
    public const int numIters = 100; //  numIters: количество итераций
    //public const int frameNr = 0;
    public const double overRelaxation = 1.9; // overRelaxation: параметр сверхрелаксации
    public const double obstacleX = .0; // obstacleX: координата препятствия по X 
    public const double obstacleY = .0; //obstacleY: координата препятствия по Y 
    //public bool paused = false;
    //public int sceneNr = 0;
    //public bool showPressure = false;
    //public bool showSmoke = true;
    public Fluid fluid;
    public bool Speed = false;
    public double a = .0;
    public int obstacle = 0;
    public double x_late = .0;
    public double y_late = .0;

    public const int U_FIELD = 0;
    public const int V_FIELD = 1;
    public const int S_FIELD = 2;

    public Fluid(double density, int numX, int numY, int h)
    {
        this.density = density; 
        this.numX = numX + 2; 
        this.numY = numY + 2; 
        this.numCells = this.numX * this.numY; 
        this.h = h; 
        this.u = new List<double>(this.numCells); 
        this.v = new List<double>(this.numCells); 
        this.newU = new List<double>(this.numCells); 
        this.newV = new List<double>(this.numCells); 
        this.p = new List<double>(this.numCells); 
        this.s = new List<double>(this.numCells); 
        this.m = new List<double>(this.numCells); 
        this.newM = new List<double>(this.numCells);
        this.m = Enumerable.Repeat(1.0, this.numCells).ToList();
        this.num = numX * numY; 
    }

    //~Fluid() { }

    //public void GetM()
    //{
    //    foreach (double n in this.m)
    //    {
    //        Console.WriteLine(n);
    //    }
    //}

    public void integrate(double dt, double gravity)
    {
        int n = this.numY;
        for (int i = 1; i < this.numX - 1; i++)
        {
            for (int j = 1; j < this.numY - 1; j++)
            {
                if (this.s[i * n + j] != 0.0 && this.s[i*n + j-1] != 0.0)
                {
                    this.v[i * n + j] += gravity * dt;
                }
            }
        }
    }
    /// <summary>
    /// Выполняет решение уравнения невозмущенности для давления
    /// </summary>
    /// <param name="numIters"></param>
    /// <param name="dt"></param>
    public void solveIncompressibility(int numIters, double dt)
    {
        int n = this.numY;
        double cp = this.density * this.h / dt;

        for (int iter = 0; iter < numIters; iter++)
        {
            for (int i = 1; i < this.numX - 1; i++)
            {
                for (int j = 1; j < this.numY - 1; j++)
                {
                    if (this.s[i*n + j] == 0.0)
                    {
                        continue;
                    }
                    double s = this.s[i * n + j];
                    double sx0 = this.s[(i - 1) * n + j];
                    double sx1 = this.s[(i + 1) * n + j];
                    double sy0 = this.s[i * n + j - 1];
                    double sy1 = this.s[i * n + j + 1];
                    s = sx0 + sx1 + sy0 + sy1;
                    if (s == 0.0) { continue; }
                    double div = this.u[(i + 1) * n + j] - this.u[i * n + j]
                        + this.v[i * n + j + 1] - this.v[i * n + j];
                    double p = -div / s;
                    p *= overRelaxation;
                    this.p[i * n + j] += cp * p;
                    this.u[i * n + j] -= sx0 * p;
                    this.u[(i + 1) * n + j] += sx1 * p;
                    this.v[i * n + j] -= sy0 * p;
                    this.v[i * n + j + 1] += sy1 * p;
                }
            }
        }
    }
    /// <summary>
    /// Выполняет экстраполяцию (продление) 
    /// значений скоростей на границах области моделирования.
    /// </summary>
    public void extrapolate()
    {
        int n = this.numY;
        for (int i = 0; i < this.numX; i++)
        {
            this.u[i * n + 0] = this.u[i * n + 1];
            this.u[i * n + this.numY - 1] = this.u[i * n + this.numY - 2];
        }
        for (int j = 0; j < this.numY; j++)
        {
            this.v[0 * n + j] = this.v[1 * n + j];
            this.v[(this.numX - 1) * n + j] = this.v[(this.numX - 2) * n + j];
        }
    }
    /// <summary>
    /// Возвращает значение поля (скорости) в указанной точке (x, y)
    /// по указанному типу поля (field). 
    /// Используется линейная интерполяция между ближайшими значениями поля.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public double sampleField(double x, double y, int field)
    {
        int n = this.numY;
        int h = this.h;
        double h1 = 1.0 / h;
        double h2 = .5 * h;
        x = Math.Max(Math.Min(x, this.numX * h), h);
        y = Math.Max(Math.Min(y, this.numY * h), h);

        double dx = .0;
        double dy = .0;

        List<double> f = new List<double>(this.numCells);

        switch (field)
        {
            case U_FIELD: f = this.u; dy = h2; break;
            case V_FIELD: f = this.v; dx = h2; break;
            case S_FIELD: f = this.m; dy = h2; dx = h2; break;
        }

        double x0 = Math.Min(Math.Floor((x - dx) * h1), this.numX - 1);
        double tx = ((x - dx) - x0 * h) * h1;
        double x1 = Math.Min(x0 + 1, this.numX - 1);

        double y0 = Math.Min(Math.Floor((y - dy) * h1), this.numY - 1);
        double ty = ((y - dy) - y0 * h) * h1;
        double y1 = Math.Min(y0 + 1, this.numY - 1);

        double sx = 1.0 - tx;
        double sy = 1.0 - ty;

        double val = sx * sy * f[(int)(x0 * n + y0)] +
            tx * sy * f[(int)(x1 * n + y0)] +
            tx * ty * f[(int)(x1 * n + y1)] +
            sx * ty * f[(int)(x0 * n + y1)];

        return val;

    }

    /// Средняя скорость в направлении X
    public double avgU (int i, int j)
    {
        int n = this.numY;
        double u = (this.u[i * n + j - 1] + this.u[i * n + j] + 
            this.u[(i + 1) * n + j - 1] + this.u[(i + 1) * n + j]) * 0.25;
        return u;
    }

    /// Средняя скорость в направлении Y
    public double avgV (int i, int j)
    {
        int n = this.numY;
        double v = (this.v[(i - 1) * n + j] + this.v[i * n + j] +
            this.v[(i - 1) * n + j + 1] + this.v[i * n + j + 1]) * 0.25;
        return v;
    }
    /// <summary>
    /// Выполняет адвекцию (транспортировку) скоростей по времени dt.
    /// Обновляет значения скоростей в новых ячейках newU и newV 
    /// на основе текущих значений скоростей u и v.
    /// Используется линейная интерполяция для получения новых значений скоростей.
    /// </summary>
    /// <param name="dt"></param>
    public void advectVel(double dt)
    {
        this.newU = this.u;
        this.newV = this.v;
        int n = this.numY;

        double h2 = .5 * h;

        for (int i = 1; i < this.numX; i++)
        {
            for (int j = 1; j < this.numY; j++)
            {

                //cnt++;

                // Горизонтальная составляющая (u)
                if (this.s[i * n + j] != 0.0 && this.s[(i - 1) * n + j] != 0.0 && j < this.numY - 1)
                {
                    double x = i * h;
                    double y = j * h + h2;
                    double u = this.u[i * n + j];
                    double v = this.avgV(i, j);
                    x = x - dt * u;
                    y = y - dt * v;
                    u = this.sampleField(x, y, U_FIELD);
                    this.newU[i * n + j] = u;
                }
                // Вертикальная составляющая (v)
                if (this.s[i * n + j] != 0.0 && this.s[i * n + j - 1] != 0.0 && i < this.numX - 1)
                {
                    double x = i * h + h2;
                    double y = j * h;
                    double u = this.avgU(i, j);
                    double v = this.v[i * n + j];
                    x = x - dt * u;
                    y = y - dt * v;
                    v = this.sampleField(x, y, V_FIELD);
                    this.newV[i * n + j] = v;
                }
            }
        }

    }
    /// <summary>
    /// выполняет адвекцию (транспортировку) дыма (массового источника) для каждой ячейки. 
    /// Обновляет новые значения массового источника newM на основе текущих 
    /// значений скоростей u и v. 
    /// Используется линейная интерполяция для получения новых значений массового источника.
    /// </summary>
    /// <param name="dt"></param>
    public void advectSmoke(double dt)
    {

        this.newM = this.m;

        int n = this.numY;
        double h = this.h;
        double h2 = 0.5 * h;

        double u, v;

        for (int i = 1; i < this.numX - 1; i++)
        {
            for (int j = 1; j < this.numY - 1; j++)
            {

                if (this.s[i * n + j] != 0.0)
                {

                    if (Speed) // Переключение числа Рейнольдса
                    {
                        u = (this.u[i * n + j] + this.u[(i + 1) * n + j]) * 0.03;   // Скорость
                        v = (this.v[i * n + j] + this.v[i * n + j + 1]) * 0.03;
                    }
                    else
                    {
                        u = (this.u[i * n + j] + this.u[(i + 1) * n + j]) * 0.5;    // Скорость
                        v = (this.v[i * n + j] + this.v[i * n + j + 1]) * 0.5;
                    }

                    var x = i * h + h2 - dt * u;
                    var y = j * h + h2 - dt * v;    // Направление

                    this.newM[i * n + j] = this.sampleField(x, y, S_FIELD);
                }
            }
        }
        this.m = this.newM;
    }

    /// <summary>
    /// Выполняет одну итерацию моделирования, симулируя движение жидкости
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="gravity"></param>
    /// <param name="numIters"></param>
    public void simulate(double dt, double gravity, int numIters)
    {
        integrate(dt, gravity);
        p = Enumerable.Repeat(.0, this.numCells).ToList();

        solveIncompressibility(numIters, dt);
        extrapolate();
        advectVel(dt);
        advectSmoke(dt);
    }
}

namespace fluid
{
    internal class Program
    {
        
        static void Main(string[] args)
        {

            

            Fluid fluid = new Fluid(1000, 100, 100, 100);
            
            //foreach (double item in fluid.m)
            //{
            //    Console.WriteLine(item);
            //}

            //fluid.GetM();
        }
    }
}

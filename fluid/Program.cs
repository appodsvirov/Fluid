using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

class Fluid
{
    #region comments
    //~Fluid() { }

    //public void GetM()
    //{
    //    foreach (double n in this.m)
    //    {
    //        Console.WriteLine(n);
    //    }
    //}
    #endregion

    #region Поля сетки
    /// <summary>Плотность жидкости </summary>
    private double density; 
    /// <summary>Количество ячеек по оси X </summary>
    private int numX;
    /// <summary>Количество ячеек по оси X </summary>
    private int numY;
    /// <summary>Общее количество ячеек в системе</summary>
    private int numCells;
    /// <summary>Размер ячейки</summary>
    private int h;
    /// <summary>Cкорости в направлении X</summary>
    private double[] u;
    /// <summary>Cкорость в направлении Y</summary>
    private double[] v;
    /// <summary>Новое значение скорости в направлении X (после dt)</summary>
    private double[] newU;
    /// <summary>Новое значение скорости в направлении Y (после dt)</summary>
    private double[] newV;
    /// <summary>Давление</summary>
    private double[] p;
    /// <summary>Массовый источник --??? to do</summary>
    private double[] s;
    /// <summary>
    /// Массовые коэффициенты каждой ячейки
    /// <para> m[i*numY + j] -- масса ячейки [i*numY + j] </para>
    /// </summary>
    private double[] m;
    /// <summary>
    /// Новые массовые коэффициенты каждой ячейки после dt 
    /// <para>newM[i*numY + j] -- масса ячейки [i*numY + j] после dt</para>
    /// </summary>
    private double[] newM;
    /// <summary>Общее количество ячеек без учета добавочных</summary>
    private int num;

    #endregion

    #region Из обьекта scene
    /// <summary>Ускорение свободного падения</summary>
    public const double gravity = 9.80665;
    /// <summary>Шаг по времени</summary>
    public const double dt = 1.0 / 120.0;
    /// <summary>Количество итераций</summary>
    public const int numIters = 100;
    /// <summary>Параметр сверхрелаксации</summary>
    public const double overRelaxation = 1.9;
    /// <summary>Координата препятствия по X </summary>
    public const double obstacleX = .0;
    /// <summary>Координата препятствия по Y </summary>
    public const double obstacleY = .0;

    //public const int frameNr = 0;
    //public bool paused = false;
    //public int sceneNr = 0;
    //public bool showPressure = false;
    //public bool showSmoke = true;

    #endregion

    #region Что за константы?
    public Fluid fluid;
    public bool Speed = false; 
    public double a = .0;
    public int obstacle = 0;
    public double x_late = .0;
    public double y_late = .0;

    #endregion

    #region enum
    public enum SearchParams
    {
        U_FIELD,
        V_FIELD, 
        S_FIELD
    }

    #endregion

    public Fluid(double density, int numX, int numY, int h)
    {
        this.density = density; 
        this.numX = numX + 2; 
        this.numY = numY + 2; 
        this.numCells = this.numX * this.numY; 
        this.h = h; 
        this.u = new double[this.numCells]; 
        this.v = new double[this.numCells]; 
        this.newU = new double[this.numCells]; 
        this.newV = new double[this.numCells]; 
        this.p = new double[this.numCells]; 
        this.s = new double[this.numCells]; 
        this.newM = new double[this.numCells];
        this.m = Enumerable.Repeat(1.0, this.numCells).ToArray();
        this.num = numX * numY; 
    }

    /// <summary>
    /// Вычисляет интеграл для ускорения свободного падения.
    /// Обновляет значения скоростей в зависимости от массового источника
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="gravity"></param>
    public void Integrate(double dt, double gravity)
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
    public void SolveIncompressibility(int numIters, double dt)
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
    public void Extrapolate()
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
    /// <para>Используется линейная интерполяция между ближайшими значениями поля</para>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public double SampleField(double x, double y, SearchParams searchParams)
    {
        int n = this.numY;
        int h = this.h;
        double h1 = 1.0 / h;
        double h2 = .5 * h;
        x = Math.Max(Math.Min(x, this.numX * h), h);
        y = Math.Max(Math.Min(y, this.numY * h), h);

        double dx = .0;
        double dy = .0;

        double[] f = new double[this.numCells];

        switch (searchParams)
        {
            case SearchParams.U_FIELD: f = this.u; dy = h2; break;
            case SearchParams.V_FIELD: f = this.v; dx = h2; break;
            case SearchParams.S_FIELD: f = this.m; dy = h2; dx = h2; break;
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

    /// <summary>
    /// Вычисляет среднюю скорость в направлении X
    /// </summary>
    /// <param name="i">Столбец</param>
    /// <param name="j">Строка</param>
    /// <returns></returns>
    public double AvgU (int i, int j)
    {
        int n = this.numY;
        double u = (this.u[i * n + j - 1] + this.u[i * n + j] + 
            this.u[(i + 1) * n + j - 1] + this.u[(i + 1) * n + j]) * 0.25;
        return u;
    }

    /// <summary>
    /// Средняя скорость в направлении Y
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    public double AvgV (int i, int j)
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
    public void AdvectVel(double dt)
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
                    double v = this.AvgV(i, j);
                    x = x - dt * u;
                    y = y - dt * v;
                    u = this.SampleField(x, y, SearchParams.U_FIELD);
                    this.newU[i * n + j] = u;
                }
                // Вертикальная составляющая (v)
                if (this.s[i * n + j] != 0.0 && this.s[i * n + j - 1] != 0.0 && i < this.numX - 1)
                {
                    double x = i * h + h2;
                    double y = j * h;
                    double u = this.AvgU(i, j);
                    double v = this.v[i * n + j];
                    x = x - dt * u;
                    y = y - dt * v;
                    v = this.SampleField(x, y, SearchParams.V_FIELD);
                    this.newV[i * n + j] = v;
                }
            }
        }

    }

    /// <summary>
    /// выполняет адвекцию (транспортировку) дыма (массового источника) для каждой ячейки. 
    /// Обновляет новые значения массового источника newM на основе текущих значений скоростей u и v. 
    /// Используется линейная интерполяция для получения новых значений массового источника.
    /// </summary>
    /// <param name="dt"></param>
    public void AdvectSmoke(double dt)
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

                    this.newM[i * n + j] = this.SampleField(x, y, SearchParams.S_FIELD);
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
    public void Simulate(int numIters) //убрал gravity, dt т.к. они есть в полях
    {
        Integrate(dt, gravity);
        p = Enumerable.Repeat(.0, this.numCells).ToArray();

        SolveIncompressibility(numIters, dt);
        Extrapolate();
        AdvectVel(dt);
        AdvectSmoke(dt);
    }

    #region Вывод + ToString()
    public override string ToString()
    {
        StringBuilder result = new StringBuilder("\nFluid:\n");
        int n = this.numY;
        result.AppendLine("u, v:");
        for (int i = 1; i < this.numX; i++)
        {

            for (int j = 1; j < this.numY; j++)
            {
                result.Append("(").Append(this.u[i * n + j]).
                    Append(",").Append(this.v[i * n + j]).Append(") ");
            }
            result.Append("\n");
        }
        result.AppendLine("newU, newV:");
        for (int i = 1; i < this.numX; i++)
        {

            for (int j = 1; j < this.numY; j++)
            {
                result.Append("(").Append(this.newU[i * n + j]).
                    Append(",").Append(this.newV[i * n + j]).Append(") ");
            }
            result.Append("\n");
        }

        result.AppendLine("m:");
        for (int i = 1; i < this.numX; i++)
        {
            for (int j = 1; j < this.numY; j++)
            {
                result.Append(this.m[i * n + j]).Append(" ");
            }
            result.Append("\n");
        }
        result.AppendLine("newM:");
        for (int i = 1; i < this.numX; i++)
        {

            for (int j = 1; j < this.numY; j++)
            {
                result.Append(this.newM[i * n + j]).Append(" " );
            }
            result.Append("\n");
        }
        return result.ToString();

    }
    #endregion

}

namespace fluid
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            Fluid fluid = new Fluid(1000, 10, 10, 100);
            fluid.Simulate(1);
            Console.WriteLine(fluid);
            //foreach (double item in fluid.m)
            //{
            //    Console.WriteLine(item);
            //}

            //fluid.GetM();
            Console.ReadLine();
        }
    }
}

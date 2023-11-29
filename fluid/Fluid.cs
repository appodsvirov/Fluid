using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace fluid
{
    public class Fluid
    {

        #region Поля, свойства сетки
        /// <summary>Плотность жидкости </summary>
        public double density;
        /// <summary>Количество ячеек по оси X </summary>
        public int numX;
        /// <summary>Количество ячеек по оси X </summary>
        public int numY;
        /// <summary>Общее количество ячеек в системе</summary>
        public int numCells;
        /// <summary>Размер ячейки</summary>
        public double h;
        /// <summary>Cкорости в направлении X</summary>
        public double[] u;
        /// <summary>Cкорость в направлении Y</summary>
        public double[] v;
        /// <summary>Новое значение скорости в направлении X (после dt)</summary>
        public double[] newU;
        /// <summary>Новое значение скорости в направлении Y (после dt)</summary>
        public double[] newV;
        /// <summary>Давление</summary>
        public double[] p;
        /// <summary>Массовый источник --??? to do</summary>
        public double[] s;
        /// <summary>
        /// Массовые коэффициенты каждой ячейки
        /// <para> m[i*numY + j] -- масса ячейки [i*numY + j] </para>
        /// </summary>
        public double[] m;
        /// <summary>
        /// Новые массовые коэффициенты каждой ячейки после dt 
        /// <para>newM[i*numY + j] -- масса ячейки [i*numY + j] после dt</para>
        /// </summary>
        public double[] newM;
        /// <summary>Общее количество ячеек без учета добавочных</summary>
        public int num;

        #endregion


        public Fluid(double density, int numX, int numY, double h)
        {
            this.density = density;
            this.numX = numX + 2;
            this.numY = numY + 2;
            this.numCells = this.numX * this.numY;
            this.h = h;
            this.u = new double[numCells];
            this.v = new double[numCells];
            this.newU = new double[numCells];
            this.newV = new double[numCells];
            this.p = new double[numCells];
            this.s = new double[numCells];
            this.m = Enumerable.Repeat(1.0, numCells).ToArray();
            this.newM = new double[numCells];
            var num = numX * numY;
        }

        /// <summary>
        /// // Гравитация
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gravity"></param>
        public void Integrate(double dt, double gravity)
        {
            var n = this.numY;
            for (var i = 1; i < this.numX; i++)
            {
                for (var j = 1; j < this.numY - 1; j++)
                {
                    //Было:
                    //if (this.s[i * n + j] != 0.0 && this.s[i * n + j - 1] != 0.0)
                    //{
                    //    this.v[i * n + j] += gravity * dt;
                    //}
                    // Комментарий: в C# так double не сравниваются.
                    if (Math.Abs(this.s[i * n + j]) > double.Epsilon &&
                        Math.Abs(this.s[i * n + j - 1]) > double.Epsilon)
                    {
                        this.v[i * n + j] += gravity * dt;
                    }
                }
            }
        }
        /// <summary>
        /// Несжимаемость
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public void SolveIncompressibility(int numIters, double dt)
        {
            var n = this.numY;
            //var cp = this.density * this.h / dt;
            var cp = 1000;

            for (var iter = 0; iter < numIters; iter++)
            {

                for (var i = 1; i < this.numX - 1; i++)
                {
                    for (var j = 1; j < this.numY - 1; j++)
                    {

                        if (this.s[i * n + j] == 0.0)
                            continue;

                        var s = this.s[i * n + j];
                        var sx0 = this.s[(i - 1) * n + j];
                        var sx1 = this.s[(i + 1) * n + j];
                        var sy0 = this.s[i * n + j - 1];
                        var sy1 = this.s[i * n + j + 1];
                        s = sx0 + sx1 + sy0 + sy1;
                        if (s == 0.0)
                        {
                            continue;
                        }
                        var div = this.u[(i + 1) * n + j] - this.u[i * n + j] +
                            this.v[i * n + j + 1] - this.v[i * n + j];

                        var p = -div / s;
                        p *= Scene.overRelaxation;


                        this.p[i * n + j] += cp * p;

                        this.u[i * n + j] -= sx0 * p;
                        this.u[(i + 1) * n + j] += sx1 * p;
                        this.v[i * n + j] -= sy0 * p;
                        this.v[i * n + j + 1] += sy1 * p;
                    }
                }
            }
        }

        public void Extrapolate()
        {                               // Экстраполяция
            var n = this.numY;
            for (var i = 0; i < this.numX; i++)
            {
                this.u[i * n + 0] = this.u[i * n + 1];
                this.u[i * n + this.numY - 1] = this.u[i * n + this.numY - 2];
            }
            for (var j = 0; j < this.numY; j++)
            {
                this.v[0 * n + j] = this.v[1 * n + j];
                this.v[(this.numX - 1) * n + j] = this.v[(this.numX - 2) * n + j];
            }
        }

        public double SampleField(double x, double y, SearchParams field)
        {
            var n = this.numY;
            var h = this.h;
            var h1 = 1.0 / h;
            var h2 = 0.5 * h;

            x = Math.Max(Math.Min(x, this.numX * h), h);
            y = Math.Max(Math.Min(y, this.numY * h), h);

            var dx = 0.0;
            var dy = 0.0;

            var dxR = 0.0;
            var dyR = 0.0;

            var a = 0.0;

            double[] f = this.u;

            switch (field)
            {
                case SearchParams.U_FIELD: f = this.u; dy = h2; break;
                case SearchParams.V_FIELD: f = this.v; dx = h2; break;
                case SearchParams.S_FIELD: f = this.m; dx = h2; dy = h2; break;

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

        public double avgU(int i, int j)
        {
            var n = this.numY;
            var u = (this.u[i * n + j - 1] + this.u[i * n + j] +
                this.u[(i + 1) * n + j - 1] + this.u[(i + 1) * n + j]) * 0.25;
            return u;

        }

        public double avgV(int i, int j)
        {
            var n = this.numY;
            var v = (this.v[(i - 1) * n + j] + this.v[i * n + j] +
                this.v[(i - 1) * n + j + 1] + this.v[i * n + j + 1]) * 0.25;
            return v;
        }

        public void AdvectVel(double dt)
        {                                      // Адвекция
            Array.Copy(u, newU, u.Length);
            Array.Copy(v, newV, v.Length);
            var n = this.numY;
            var h = this.h;
            var h2 = 0.5 * h;

            for (var i = 1; i < this.numX; i++)
            {
                for (var j = 1; j < this.numY; j++)
                {
                    //cnt++;

                    // Горизонтальная составляющая (u)
                    if (this.s[i * n + j] != 0.0 && this.s[(i - 1) * n + j] != 0.0 && j < this.numY - 1)
                    {
                        var x = (i * h);
                        var y = (j * h + h2);
                        var u = this.u[i * n + j];
                        var v = this.avgV(i, j);
                        x = (x - dt * u);
                        y = (y - dt * v);
                        u = this.SampleField(x, y, SearchParams.U_FIELD);
                        this.newU[i * n + j] = u;
                    }
                    // Вертикальная составляющая (v)
                    if (this.s[i * n + j] != 0.0 && this.s[i * n + j - 1] != 0.0 && i < this.numX - 1)
                    {
                        var x = (i * h + h2);
                        var y = (j * h);
                        var u = this.avgU(i, j);
                        var v = this.v[i * n + j];
                        x = (x - dt * u);
                        y = (y - dt * v);
                        v = this.SampleField(x, y, SearchParams.V_FIELD);
                        this.newV[i * n + j] = v;
                    }
                }
            }
            Array.Copy(newU, u, u.Length);
            Array.Copy(newV, v, v.Length);
        }

        public void AdvectSmoke(double dt)
        {
            Array.Copy(m, newM, m.Length);

            var n = this.numY;
            var h = this.h;
            var h2 = 0.5 * h;

            for (var i = 1; i < this.numX - 1; i++)
            {
                for (var j = 1; j < this.numY - 1; j++)
                {

                    if (this.s[i * n + j] != 0.0)
                    {
                        double u, v;
                        if (Scene.Speed) // Переключение числа Рейнольдса
                        {
                            u = (this.u[i * n + j] + this.u[(i + 1) * n + j]) * 0.03;   // Скорость
                            v = (this.v[i * n + j] + this.v[i * n + j + 1]) * 0.03;
                        }
                        else
                        {
                            u = (this.u[i * n + j] + this.u[(i + 1) * n + j]) * 0.5;    // Скорость
                            v = (this.v[i * n + j] + this.v[i * n + j + 1]) * 0.5;
                        }

                        var x = (i * h + h2 - dt * u);
                        var y = (j * h + h2 - dt * v);    // Направление

                        this.newM[i * n + j] = this.SampleField(x, y, SearchParams.S_FIELD);
                    }
                }
            }
            Array.Copy(newM, m, m.Length);
        }

        // Конец симуляции

        public void Simulate(double dt, double gravity, int numIters, LogFluid lf)
        {
            if (lf != null){
                int n = numY;
                lf.Log(p[lf.pointUp.Item1 * numY + lf.pointUp.Item2],
                    p[lf.pointDown.Item1 * numY + lf.pointDown.Item2],
                    m[lf.pointUp.Item1 * numY + lf.pointUp.Item2],
                    m[lf.pointDown.Item1 * numY + lf.pointDown.Item2],
                    Math.Sqrt(Math.Pow(u[lf.pointUp.Item1 * numY + lf.pointUp.Item2], 2) * Math.Pow(v[lf.pointUp.Item1 * numY + lf.pointUp.Item2], 2)),
                    Math.Sqrt(Math.Pow(u[lf.pointDown.Item1 * numY + lf.pointDown.Item2], 2) * Math.Pow(v[lf.pointDown.Item1 * numY + lf.pointDown.Item2], 2))
                    );
            }

            this.Integrate(dt, gravity);

            this.p = new double[p.Length];
            this.SolveIncompressibility(numIters, dt);

            this.Extrapolate();
            this.AdvectVel(dt);
            this.AdvectSmoke(dt);
        }
    }
}

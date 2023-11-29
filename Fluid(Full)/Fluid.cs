using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid_Full_
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
        /// <summaty>// Scene: Переключение числа Рейнольдса -- ? </summaty>
        private bool Speed = false;

        //public const int frameNr = 0;
        //public bool paused = false;
        //public int sceneNr = 0;
        //public bool showPressure = false;
        //public bool showSmoke = true;

        #endregion


        public Fluid(double density, int numX, int numY, double h)
        {
            this.density = density;
            this.numX = numX + 2;
            this.numY = numY + 2;
            this.numCells = this.numX * this.numY;
            this.h = h;
            this.u = new double[this.numCells];
            this.v = new double[this.numCells];
            this.p = new double[this.numCells];
            this.s = new double[this.numCells];
            this.m = Enumerable.Repeat(1.0, this.numCells).ToArray();
            this.newU = new double[this.numCells];
            this.newV = new double[this.numCells];
            this.newM = new double[this.numCells];
            this.num = numX * numY;
        }
        /// <summary>
        /// // Гравитация   
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
                    if (this.s[i * n + j] != 0.0 && this.s[i * n + j - 1] != 0.0)
                    {
                        this.v[i * n + j] += gravity * dt;
                    }
                }
            }
        }

        /// <summary>
        /// Несжимаемость
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
                        if (this.s[i * n + j] == 0.0)
                        {
                            continue;
                        }
                        double s = this.s[i * n + j];
                        double sx0 = this.s[(i - 1) * n + j];
                        double sx1 = this.s[(i + 1) * n + j];
                        double sy0 = this.s[i * n + j - 1];
                        double sy1 = this.s[i * n + j + 1];
                        s = sx0 + sx1 + sy0 + sy1;
                        if (s == 0.0)
                        { 
                            continue;
                        }
                        double div = this.u[(i + 1) * n + j] - this.u[i * n + j]
                            + this.v[i * n + j + 1] - this.v[i * n + j];
                        double p = -div / s;
                        p *= overRelaxation; //1.0 из scane; 2.0 поле клааса Fluid
                        this.p[i * n + j] += cp * p;
                        this.u[i * n + j] -= sx0 * p;
                        this.u[(i + 1) * n + j] += sx1 * p;
                        this.v[i * n + j] -= sy0 * p;
                        this.v[i * n + j + 1] += sy1 * p;
                    }
                }

                //Point coorinate;
                //coorinate = LogFluid.GetCoordinate(0);
                //LogFluid.AddSpeeds(0, u[coorinate.X * n + coorinate.Y], v[coorinate.X * n + coorinate.Y]);
                //LogFluid.AddMass(0, m[coorinate.X * n + coorinate.Y]);

                //coorinate = LogFluid.GetCoordinate(1);
                //LogFluid.AddSpeeds(1, u[coorinate.X * n + coorinate.Y], v[coorinate.X * n + coorinate.Y]);
                //LogFluid.AddMass(1, m[coorinate.X * n + coorinate.Y]);
            }
        }
        /// <summary>
        /// Выполняет экстраполяцию (продление) значений скоростей на границах области моделирования.
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
            double h = this.h;
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
        /// <param name="i">Строка</param>
        /// <param name="j">Столбец</param>
        /// <returns></returns>
        public double AvgU(int i, int j)
        {
            int n = this.numY;
            double u = (this.u[i * n + j - 1] + this.u[i * n + j] +
                this.u[(i + 1) * n + j - 1] + this.u[(i + 1) * n + j]) * 0.25;
            return u;
        }

        /// <summary>
        /// Средняя скорость в направлении Y
        /// </summary>
        /// <param name="i">Строка</param>
        /// <param name="j">Столбец</param>
        /// <returns></returns>
        public double AvgV(int i, int j)
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

        public void Simulate(double dt = dt, double gravity = gravity, int numIters = numIters)
        {
            int n = numY;
            Point coorinate;
            coorinate = LogFluid.GetCoordinate(0);
            LogFluid.AddSpeeds(0, newU[coorinate.X * n + coorinate.Y], newV[coorinate.X * n + coorinate.Y]);
            LogFluid.AddMass(0, newM[coorinate.X * n + coorinate.Y]);
            LogFluid.AddP(0, p[coorinate.X * n + coorinate.Y]);

            coorinate = LogFluid.GetCoordinate(1);
            LogFluid.AddSpeeds(1, newU[coorinate.X * n + coorinate.Y], newV[coorinate.X * n + coorinate.Y]);
            LogFluid.AddMass(1, newM[coorinate.X * n + coorinate.Y]);
            LogFluid.AddP(1, p[coorinate.X * n + coorinate.Y]);

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
            StringBuilder result = new StringBuilder("Fluid:\n");
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
                    result.Append(this.newM[i * n + j]).Append(" ");
                }
                result.Append("\n");
            }
            return result.ToString();

        }
        #endregion
    }


}

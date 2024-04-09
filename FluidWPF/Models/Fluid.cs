using FluidWPF.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FluidWPF.Models
{
    public class Fluid
    {
        public int density;
        public int numX;
        public int numY;
        public int numCells;
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
        bool solveTurbulence = true;

        public double overRelaxation = 1.9;

        private BaldwinLomaxModel _turbulence;
        public Fluid(int density, int numX, int numY, double h, bool st)
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
            this.solveTurbulence = st;

            num = numX * numY;

            _turbulence = new(this); 
        }

        public void Integrate(double dt, double gravity) // Гравитация
        {
            var n = this.numY;
            for (var i = 1; i < this.numX; i++)
            {
                for (var j = 1; j < this.numY - 1; j++)
                {
                    if (this.s[(i * n + j)] != 0.0 && this.s[(i * n + j - 1)] != 0.0)
                    {
                        this.v[(i * n + j)] += gravity * dt;
                    }
                }
            }
        }


        public void SolveIncompressibility(int numIters, double dt) // Несжимаемость
        {

            int n = this.numY;
            double cp = this.density * this.h / dt;

            for (var iter = 0; iter < numIters; iter++)
            {

                for (var i = 1; i < this.numX - 1; i++)
                {
                    for (var j = 1; j < this.numY - 1; j++)
                    {

                        if (this.s[(i * n + j)] == 0.0)
                        {
                            continue;
                        }

                        var s = this.s[(i * n + j)];
                        var sx0 = this.s[((i - 1) * n + j)];
                        var sx1 = this.s[((i + 1) * n + j)];
                        var sy0 = this.s[(i * n + j - 1)];
                        var sy1 = this.s[(i * n + j + 1)];
                        s = sx0 + sx1 + sy0 + sy1;
                        if (s == 0.0)
                        {
                            continue;
                        }

                        var div = this.u[((i + 1) * n + j)] - this.u[(i * n + j)] +
                            this.v[(i * n + j) + 1] - this.v[(i * n + j)];

                        var p = -div / s;
                        p *= overRelaxation;


                        this.p[(i * n + j)] += cp * p;

                        this.u[(i * n + j)] -= sx0 * p;
                        this.u[((i + 1) * n + j)] += sx1 * p;
                        this.v[(i * n + j)] -= sy0 * p;
                        this.v[(i * n + j) + 1] += sy1 * p;
                    }
                }
            }
        }


        public void Extrapolate() // Экстраполяция
        {
            int n = this.numY;
            for (var i = 0; i < this.numX; i++)
            {
                this.u[(i * n + 0)] = this.u[(i * n + 1)];
                this.u[(i * n + this.numY - 1)] = this.u[(i * n + this.numY - 2)];
            }
            for (var j = 0; j < this.numY; j++)
            {
                this.v[(0 * n + j)] = this.v[(1 * n + j)];
                this.v[((this.numX - 1) * n + j)] = this.v[((this.numX - 2) * n + j)];

            }
        }

        public double SampleField(double x, double y, SearchValue field)
        {
            int n = this.numY;
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

            var x0 = (int)Math.Min(Math.Floor((x - dx) * h1), this.numX - 1);
            var tx = ((x - dx) - x0 * h) * h1;
            var x1 = (int) Math.Min(x0 + 1, this.numX - 1);

            var y0 = (int)Math.Min(Math.Floor((y - dy) * h1), this.numY - 1);
            var ty = ((y - dy) - y0 * h) * h1;
            var y1 = (int)Math.Min(y0 + 1, this.numY - 1);

            var sx = 1.0 - tx;
            var sy = 1.0 - ty;

            var val = sx * sy * f[(x0 * n + y0)] +
                tx * sy * f[(x1 * n + y0)] +
                tx * ty * f[(x1 * n + y1)] +
                sx * ty * f[(x0 * n + y1)];

            return val;
        }

        public double AvgU(int i, int j)
        {
            var n = this.numY;
            var u = (this.u[(i * n + j - 1)] + this.u[(i * n + j)] +
                this.u[((i + 1) * n + j - 1)] + this.u[((i + 1) * n + j)]) * 0.25;

            if (solveTurbulence)
            {
                var dynamicU = _turbulence.SolveDynamicSpeed(
                    this.u[i * n + j],
                    u,
                    j,
                    this.u,
                    this.v
                    );
                return u + dynamicU;
            }
            return u;
        }

        public double AvgV(int i, int j, bool solveTurbulence = true)
        {
            var n = this.numY;
            var v = (this.v[((i - 1) * n + j)] + this.v[(i * n + j)] +
                this.v[((i - 1) * n + j) + 1] + this.v[(i * n + j) + 1]) * 0.25;
            return v;
        }

        public void AdvectVel(double dt) // Адвекция
        {
            Array.Copy(this.u, newU, this.u.Length); //this.newU.set(this.u); 
            Array.Copy(this.v, newV, this.v.Length); //this.newV.set(this.v);

            int n = this.numY;
            double h = this.h;
            double h2 = 0.5 * h;

            for (int i = 1; i < this.numX; i++)
            {
                for (int j = 1; j < this.numY; j++)
                {


                    // Горизонтальная составляющая (u)
                    if (this.s[(i * n + j)] != 0.0 && this.s[((i - 1) * n + j)] != 0.0 && j < this.numY - 1)
                    {
                        var x = i * h;
                        var y = j * h + h2;
                        var u = this.u[(i * n + j)];
                        var v = this.AvgV(i, j);
                        x = x - dt * u;
                        y = y - dt * v;
                        u = this.SampleField(x, y, SearchValue.U_FIELD);
                        this.newU[(i * n + j)] = u;
                    }
                    // Вертикальная составляющая (v)
                    if (this.s[(i * n + j)] != 0.0 && this.s[(i * n + j) - 1] != 0.0 && i < this.numX - 1)
                    {
                        var x = i * h + h2;
                        var y = j * h;
                        var u = this.AvgU(i, j);
                        var v = this.v[(i * n + j)];
                        x = x - dt * u;
                        y = y - dt * v;
                        v = this.SampleField(x, y, SearchValue.V_FIELD);
                        this.newV[(i * n + j)] = v;
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

                    if (this.s[(i * n + j)] != 0.0)
                    {

                        double u, v;
                        if (false) // Переключение числа Рейнольдса
                        {
                            u = (this.u[(i * n + j)] + this.u[((i + 1) * n + j)]) * 0.03;   // Скорость
                            v = (this.v[(i * n + j)] + this.v[(i * n + j + 1)]) * 0.03;
                        }
                        else
                        {
                            u = (this.u[(i * n + j)] + this.u[((i + 1) * n + j)]) * 0.5;    // Скорость
                            v = (this.v[(i * n + j)] + this.v[(i * n + j + 1)]) * 0.5;
                        }

                        double x = i * h + h2 - dt * u;
                        double y = j * h + h2 - dt * v;    // Направление

                        this.newM[(i * n + j)] = this.SampleField(x, y, SearchValue.S_FIELD);
                    }
                }
            }
            Array.Copy(this.newM, this.m, this.newM.Length); //this.m.set(this.newM);
        }

        public void Simulate(double dt, double gravity, int numIters,  double cnt, LogVerification lf, int iterNum)
        {
            this.Integrate(dt, gravity);

            this.p = Enumerable.Repeat(0.0, (this.numCells)).ToArray();//this.p.fill(0.0);
            this.SolveIncompressibility(numIters, dt);

            this.Extrapolate();
            this.AdvectVel(dt);
            this.AdvectSmoke(dt);

            if (lf != null && iterNum >= lf.start && iterNum % lf.dlog == 0)
            {
                double n = numY;
                lf.Log(p[(lf.pointUp.Item1 * numY + lf.pointUp.Item2)]);
            }
        }
    }
}


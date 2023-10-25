using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid_Full_
{
    public class Fluid
    {
        public float[] u;
        public float[] v;
        public float[] newU;
        public float[] newV;
        public float[] p;
        public float[] s;
        public float[] m;
        public float[] newM;
        public float density;
        public int numX;
        public int numY;
        public int numCells;
        public float h;

        public Fluid(float density, int numX, int numY, float h)
        {
            this.density = density;
            this.numX = numX + 2;
            this.numY = numY + 2;
            this.numCells = this.numX * this.numY;
            this.h = h;
            this.u = new float[this.numCells];
            this.v = new float[this.numCells];
            this.newU = new float[this.numCells];
            this.newV = new float[this.numCells];
            this.p = new float[this.numCells];
            this.s = new float[this.numCells];
            this.m = new float[this.numCells];
            this.newM = new float[this.numCells];
            Array.Fill(this.m, 1.0f);
        }

        public void Integrate(float dt, float gravity)
        {
            int n = this.numY;
            for (int i = 1; i < this.numX; i++)
            {
                for (int j = 1; j < this.numY - 1; j++)
                {
                    int index = i * n + j;
                    if (this.s[index] != 0.0 && this.s[index + n] != 0.0)
                        this.v[index] += gravity * dt;
                }
            }
        }

        public void SolveIncompressibility(int numIters, float dt)
        {
            int n = this.numY;
            float cp = this.density * this.h / dt;
            for (int iter = 0; iter < numIters; iter++)
            {
                for (int i = 1; i < this.numX - 1; i++)
                {
                    for (int j = 1; j < this.numY - 1; j++)
                    {
                        int index = i * n + j;
                        if (this.s[index] == 0.0)
                            continue;
                        float s = this.s[index];
                        float sx0 = this.s[(i - 1) * n + j];
                        float sx1 = this.s[(i + 1) * n + j];
                        float sy0 = this.s[index - n];
                        float sy1 = this.s[index + n];
                        s = sx0 + sx1 + sy0 + sy1;
                        if (s == 0.0)
                            continue;
                        float div = this.u[(i + 1) * n + j] - this.u[index] + this.v[index + 1] - this.v[index];
                        float p = -div / s;
                        p *= scene.overRelaxation;
                        this.p[index] += cp * p;
                        this.u[index] -= sx0 * p;
                        this.u[(i + 1) * n + j] += sx1 * p;
                        this.v[index] -= sy0 * p;
                        this.v[index + 1] += sy1 * p;
                    }
                }
            }
        }

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

        public float SampleField(float x, float y, int field)
        {
            int n = this.numY;
            float h = this.h;
            float h1 = 1.0f / h;
            float h2 = 0.5f * h;
            x = Math.Max(Math.Min(x, this.numX * h), h);
            y = Math.Max(Math.Min(y, this.numY * h), h);
            float dx = 0.0f;
            float dy = 0.0f;
            float dxR = 0.0f;
            float dyR = 0.0f;
            float a = 0.0f;
            float f = 0.0f;
            switch (field)
            {
                case U_FIELD:
                    f = this.u;
                    dy = h2;
                    break;

                case V_FIELD:
                    f = this.v;
                    dx = h2;
                    break;
                case S_FIELD:
                    f = this.m;
                    dx = h2;
                    dy = h2;
                    break;
            }
            int x0 = Math.Min((int)Math.Floor((x - dx) * h1), this.numX - 1);
            float tx = ((x - dx) - x0 * h) * h1;
            int x1 = Math.Min(x0 + 1, this.numX - 1);
            int y0 = Math.Min((int)Math.Floor((y - dy) * h1), this.numY - 1);
            float ty = ((y - dy) - y0 * h) * h1;
            int y1 = Math.Min(y0 + 1, this.numY - 1);
            float sx = 1.0f - tx;
            float sy = 1.0f - ty;
            float val = sx * sy * f[x0 * n + y0] +
                        tx * sy * f[x1 * n + y0] +
                        tx * ty * f[x1 * n + y1] +
                        sx * ty * f[x0 * n + y1];
            return val;
        }

        public float AvgU(int i, int j)
        {
            int n = this.numY;
            float u = (this.u[i * n + j - 1] + this.u[i * n + j] +
                        this.u[(i + 1) * n + j - 1] + this.u[(i + 1) * n + j]) * 0.25f;
            return u;
        }

        public float AvgV(int i, int j)
        {
            int n = this.numY;
            float v = (this.v[(i - 1) * n + j] + this.v[i * n + j] +
                        this.v[(i - 1) * n + j + 1] + this.v[i * n + j + 1]) * 0.25f;
            return v;
        }

        public void AdvectVel(float dt)
        {
            this.newU = (float[])this.u.Clone();
            this.newV = (float[])this.v.Clone();
            int n = this.numY;
            float h = this.h;
            float h2 = 0.5f * h;
            for (int i = 1; i < this.numX; i++)
            {
                for (int j = 1; j < this.numY; j++)
                {
                    cnt++;
                    if (this.s[i * n + j] != 0.0 && this.s[(i - 1) * n + j] != 0.0 && j < this.numY - 1)
                    {
                        float x = i * h;
                        float y = j * h + h2;
                        float u = this.u[i * n + j];
                        float v = this.AvgV(i, j);
                        x = x - dt * u;
                        y = y - dt * v;
                        u = this.SampleField(x, y, U_FIELD);
                        this.newU[i * n + j] = u;
                    }
                    if (this.s[i * n + j] != 0.0 && this.s[i * n + j - 1] != 0.0 && i < this.numX - 1)
                    {
                        float x = i * h + h2;
                        float y = j * h;
                        float u = this.AvgU(i, j);
                        float v = this.v[i * n + j];
                        x = x - dt * u;
                        y = y - dt * v;
                        v = this.SampleField(x, y, V_FIELD);
                        this.newV[i * n + j] = v;
                    }
                }
            }
            this.u = (float[])this.newU.Clone();
            this.v = (float[])this.newV.Clone();
        }

        public void AdvectSmoke(float dt)
        {
            this.newM = (float[])this.m.Clone();
            int n = this.numY;
            float h = this.h;
            float h2 = 0.5f * h;
            for (int i = 1; i < this.numX - 1; i++)
            {
                for (int j = 1; j < this.numY - 1; j++)
                {
                    if (this.s[i * n + j] != 0.0)
                    {
                        if (scene.Speed) // Переключение числа Рейнольдса
                        {
                            float u = (this.u[i * n + j] + this.u[(i + 1) * n + j]) * 0.03f; // Скорость
                            float v = (this.v[i * n + j] + this.v[i * n + j + 1]) * 0.03f;
                        }
                        else
                        {
                            float u = (this.u[i * n + j] + this.u[(i + 1) * n + j]) * 0.5f; // Скорость
                            float v = (this.v[i * n + j] + this.v[i * n + j + 1]) * 0.5f;
                        }
                        float x = i * h + h2 - dt * u;
                        float y = j * h + h2 - dt * v;
                        this.newM[i * n + j] = this.SampleField(x, y, S_FIELD);
                    }
                }
            }
            this.m = (float[])this.newM.Clone();
        }

        public void Simulate(float dt, float gravity, int numIters)
        {
            this.Integrate(dt, gravity);
            Array.Fill(this.p, 0.0f);
            this.SolveIncompressibility(numIters, dt);
            this.Extrapolate();
            this.AdvectVel(dt);
            this.AdvectSmoke(dt);
        }
    }


}

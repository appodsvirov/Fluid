using FluidWPF.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Threading;
using System.Windows;

namespace FluidWPF.ViewModels
{
    public partial class MainViewModel
    {

        private void UpdateReynolds()
        {
            Reynolds = ((1000 * InSpeed * Height) / 1E-03).ToString("E");
        }

        private int OnSolveRequestedAdd()
        {
            var tmp = new Variables(Variables.count.ToString() + ": " + Reynolds.ToString());
            _solveRequested?.Invoke(this, tmp);
            return tmp.Id;
        }
        private void OnSolveRequestedUpdate(int id, int progress, double seconds)
        {
            _solveRequested?.Invoke(this, new Variables(id,progress, seconds));
        }
        public void Solve(object status)
        {
            InProggress = true;
            int id = OnSolveRequestedAdd();
            LogVerification logVerification = Is4Сylinders? new LogVerification(Dlog, Start, (115, 60)) : new LogVerification(Dlog, Start, Height, ScaleNet);
            SolverFluid solverFluid = new SolverFluid(1.0 / Dt,InSpeed, Rad2, ScaleNet, IsSolveTurbulence, Is4Сylinders? 44:0);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int countSteps = CountSteps;

            for (int i = 0; i < countSteps; i++)
            {
                if (((CancellationToken)status).IsCancellationRequested)
                {
                    InProggress = false;
                    return;
                }

                solverFluid.Simulate(i, logVerification);// Solve
                if (!Saving)
                {
                    //Dispatcher.Invoke(() => ProgressStatus = (int)(100 * (i + 1) / CountSteps));
                    //Dispatcher.Invoke(() => Time = stopwatch.ElapsedMilliseconds / 1000.0);
                    OnSolveRequestedUpdate(id, (int)(100 * (i + 1) / countSteps), stopwatch.ElapsedMilliseconds / 1000.0);
                }
            }
            if (Saving)
            {
                Saving = false;
                //Dispatcher.Invoke(() => ProgressStatus = 100);
                //Dispatcher.Invoke(() => Time = stopwatch.ElapsedMilliseconds / 1000.0);
            }
            stopwatch.Stop();
            logVerification.GetLog($"P{id}.txt");
            InProggress = false;

        }
    }
}

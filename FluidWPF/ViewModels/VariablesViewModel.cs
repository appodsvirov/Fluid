using FluidWPF.Abstract;
using FluidWPF.Models;
using FluidWPF.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FluidWPF.ViewModels
{
    public class VariablesViewModel : BaseVM
    {
        #region Вводимые переменные
        public double Dt
        {
            get { return (double)GetValue(DtProperty); }
            set { SetValue(DtProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Dt.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DtProperty =
            DependencyProperty.Register("Dt", typeof(double), typeof(VariablesViewModel), new PropertyMetadata(0.01667));

        public double InSpeed
        {
            get { return (double)GetValue(InSpeedProperty); }
            set { SetValue(InSpeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InSpeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InSpeedProperty =
            DependencyProperty.Register("InSpeed", typeof(double), typeof(VariablesViewModel), new PropertyMetadata(1.0));
        public double Rad2 { get; set; }
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set
            {
                Rad2 = (value / 2) * (value / 2);
                SetValue(HeightProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(VariablesViewModel), new PropertyMetadata(0.2));




        public int CountSteps
        {
            get { return (int)GetValue(CountStepsProperty); }
            set { SetValue(CountStepsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CountSteps.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountStepsProperty =
            DependencyProperty.Register("CountSteps", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(0));

        public int ScaleNet
        {
            get { return (int)GetValue(ScaleNetProperty); }
            set { SetValue(ScaleNetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScaleNet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleNetProperty =
            DependencyProperty.Register("ScaleNet", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(1));



        public int Dlog
        {
            get { return (int)GetValue(DlogProperty); }
            set { SetValue(DlogProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Dlog.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DlogProperty =
            DependencyProperty.Register("Dlog", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(500));



        #endregion

        #region Кнопки + команды + ПрогрессБар   

        private Thread _thread;
        private CancellationTokenSource _tokenSource;

        public int ProgressStatus
        {
            get { return (int)GetValue(ProgressStatusProperty); }
            set { SetValue(ProgressStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressStatusProperty =
            DependencyProperty.Register("ProgressStatus", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(0));



        private bool inProggress = false;
        public bool InProggress { get => inProggress;
            set 
            {
                IsFree = !value;
                inProggress = value;
                OnPropertyChanged();
            } 
        }

        private bool isFree = true;
        public bool IsFree
        {
            get => isFree;
            set
            {
                isFree = value;
                OnPropertyChanged();
            }
        }

        public double Time
        {
            get { return (double)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(double), typeof(VariablesViewModel), new PropertyMetadata(0.0));

        public void Solve(object status)
        {
            InProggress = true;
            LogVerification logVerification = new LogVerification(Dispatcher.Invoke(() => Dlog));
            Dispatcher.Invoke(() => ProgressStatus = 0);
            SolverFluid solverFluid = new SolverFluid(
                Dispatcher.Invoke(() => Dt), 
                Dispatcher.Invoke(() => InSpeed),
                Dispatcher.Invoke(() => Rad2),
                Dispatcher.Invoke(() => ScaleNet));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < Dispatcher.Invoke(() => CountSteps); i++)
            {
                if (((CancellationToken)status).IsCancellationRequested)
                {
                    InProggress = false;
                    return;
                }

                solverFluid.Simulate(i, logVerification);// Solve

                Dispatcher.Invoke(() => ProgressStatus = (int)(100 * (i + 1) / CountSteps));
                Dispatcher.Invoke(() => Time = stopwatch.ElapsedMilliseconds / 1000.0);
            }

            stopwatch.Stop();
            logVerification.GetLog();
            InProggress = false;
        }

        public ICommand ClikToSolve
        {
            get
            {
                return new DelegateCommand((p) =>
                {
                    _tokenSource = new CancellationTokenSource();
                    _thread = new Thread(Solve) { IsBackground = true };
                    _thread.Start(_tokenSource.Token);
                });
            }
        }

        public ICommand ClikToStop
        {
            get
            {
                return new DelegateCommand((p) =>
                {
                    _tokenSource.Cancel();
                    _tokenSource = null;
                    _thread = null;
                });
            }
        }

        #endregion
    }
}

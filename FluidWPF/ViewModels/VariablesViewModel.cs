using FluidWPF.Abstract;
using FluidWPF.Models;
using FluidWPF.Utilities;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FluidWPF.ViewModels
{
    public class VariablesViewModel : BaseVM
    {
        #region Вводимые переменные
        public int Dt
        {
            get { return (int)GetValue(DtProperty); }
            set { SetValue(DtProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Dt.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DtProperty =
            DependencyProperty.Register("Dt", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(60, UpdateReynolds));

        public double InSpeed
        {
            get { return (double)GetValue(InSpeedProperty); }
            set
            {
                SetValue(InSpeedProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for InSpeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InSpeedProperty =
            DependencyProperty.Register("InSpeed", typeof(double), typeof(VariablesViewModel), new PropertyMetadata(1.0, UpdateReynolds));

        public double Rad2 { get; set; } = 0.01;
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
            DependencyProperty.Register("Height", typeof(double), typeof(VariablesViewModel), new PropertyMetadata(0.1, (d, e) =>
            {
                VariablesViewModel vm = (VariablesViewModel)d;
                if (vm != null)
                {
                    vm.Rad2 = (vm.Height / 2) * (vm.Height / 2);
                    UpdateReynolds(d, e);
                }
            }));

        public int CountSteps
        {
            get { return (int)GetValue(CountStepsProperty); }
            set { SetValue(CountStepsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CountSteps.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountStepsProperty =
            DependencyProperty.Register("CountSteps", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(2000));

        public int ScaleNet
        {
            get { return (int)GetValue(ScaleNetProperty); }
            set { SetValue(ScaleNetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScaleNet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleNetProperty =
            DependencyProperty.Register("ScaleNet", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(1, UpdateReynolds));



        public int Dlog
        {
            get { return (int)GetValue(DlogProperty); }
            set { SetValue(DlogProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Dlog.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DlogProperty =
            DependencyProperty.Register("Dlog", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(1));



        public int Start
        {
            get { return (int)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(0));

        public string Reynolds
        {
            get { return (string)GetValue(ReynoldsProperty); }
            set { SetValue(ReynoldsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Reynolds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReynoldsProperty =
            DependencyProperty.Register("Reynolds", typeof(string), typeof(VariablesViewModel), new PropertyMetadata(1E5.ToString("E")));

        private static void UpdateReynolds(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VariablesViewModel vm = (VariablesViewModel)d;
            if (vm != null)
            {
                // to do: захардкодил плотность и вязкость. Сделать события!
                vm.Reynolds = ((1000 * vm.InSpeed * vm.Height) / 1E-03).ToString("E");

                //Math.Round((
                //    1000 * vm.InSpeed * vm.Height /
                //    ((1000.0 * Math.Pow((1.0 / (100.0 * vm.ScaleNet)), 4) / (1.0 / vm.Dt)))
                //), 2)
                //.ToString("E");
            } // 1.002E-03 / Math.Pow(this.h, 3);
        }

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

        [Reactive] public bool IsSolveTurbulence { get; set; } = false;

        private bool inProggress = false;
        public bool InProggress
        {
            get => inProggress;
            set
            {
                inProggress = value;
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

        private bool saving = false;
        public bool Saving
        {
            get => saving;
            set
            {
                saving = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<Variables> SolveRequested;
        private int OnSolveRequestedAdd()
        {
            var tmp = new Variables( Variables.count.ToString() + ": " +
                    Dispatcher.Invoke(() => Reynolds.ToString()));
            SolveRequested?.Invoke(this, tmp);
            return tmp.Id;
        }
        private void OnSolveRequestedUpdate(int id, int progress)
        {
            SolveRequested?.Invoke(this,
                new Variables(
                    id,
                    progress));
        }

        public void Solve(object status)
        {
            InProggress = true;
            int id = OnSolveRequestedAdd();
            LogVerification logVerification = new LogVerification(
                Dispatcher.Invoke(() => Dlog),
                Dispatcher.Invoke(() => Start),
                Dispatcher.Invoke(() => Height),
                Dispatcher.Invoke(() => ScaleNet)
                );
            Dispatcher.Invoke(() => ProgressStatus = 0);
            SolverFluid solverFluid = new SolverFluid(
                Dispatcher.Invoke(() => 1.0 / Dt),
                Dispatcher.Invoke(() => InSpeed),
                Dispatcher.Invoke(() => Rad2),
                Dispatcher.Invoke(() => ScaleNet),
                IsSolveTurbulence,
                0);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int countSteps = Dispatcher.Invoke(() => CountSteps);

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
                    OnSolveRequestedUpdate(id, (int)(100 * (i + 1) / countSteps));
                }
            }
            if (Saving)
            {
                Saving = false;
                Dispatcher.Invoke(() => ProgressStatus = 100);
                Dispatcher.Invoke(() => Time = stopwatch.ElapsedMilliseconds / 1000.0);
            }
            stopwatch.Stop();
            logVerification.GetLog($"P{id}.txt");
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

        public ICommand ClikToCopy
        {
            get
            {
                return new DelegateCommand((p) =>
                {
                    Clipboard.SetText(Reynolds);
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

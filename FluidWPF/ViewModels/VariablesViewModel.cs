using FluidWPF.Abstract;
using FluidWPF.Models;
using FluidWPF.Utilities;
using System;
using System.Collections.Generic;
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

        public static readonly DependencyProperty DtProperty =
            DependencyProperty.Register("Dt", typeof(double), typeof(VariablesViewModel), new PropertyMetadata(0.0167));

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


        private int countSteps = 6;
        public int CountSteps
        {
            get => countSteps;
            set
            {
                countSteps = (value == 0)? 1: (value < 0)? - value : value;
                OnPropertyChanged();
            }
        }
        public int ScaleNet
        {
            get { return (int)GetValue(ScaleNetProperty); }
            set { SetValue(ScaleNetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScaleNet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleNetProperty =
            DependencyProperty.Register("ScaleNet", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(1));
        #endregion

        #region Кнопки + команды + ПрогрессБар   

        private Thread _thread;
        private CancellationTokenSource _tokenSource;

        private int progressStatus = 0;
        public int ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                OnPropertyChanged();
            }
        }
        
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

        public void Solve(object status)
        {
            InProggress = true;
            LogVerification logVerification = new LogVerification();
            ProgressStatus = 0;
            for (int i = 0; i < CountSteps; i++)
            {
                if (((CancellationToken)status).IsCancellationRequested)
                {
                    InProggress = false;
                    return;
                }
                    
                Thread.Sleep(1000);
                ProgressStatus = (int)(100*(i+1)/CountSteps);
            }

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

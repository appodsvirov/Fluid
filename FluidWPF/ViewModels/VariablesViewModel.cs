using FluidWPF.Abstract;
using FluidWPF.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FluidWPF.ViewModels
{
    public class VariablesViewModel : BaseVM
    {
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



        public int CountSteps
        {
            get { return (int)GetValue(CountStepsProperty); }
            set { SetValue(CountStepsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CountSteps.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountStepsProperty =
            DependencyProperty.Register("CountSteps", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(1001, CheckCountSteps));

        private static void CheckCountSteps(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var current = d as VariablesViewModel;
            if(current != null && current.CountSteps < 0)
            {
                current.CountSteps *= -1;
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



        public int ProgressStatus
        {
            get { return (int)GetValue(ProgressStatusProperty); }
            set { SetValue(ProgressStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressStatusProperty =
            DependencyProperty.Register("ProgressStatus", typeof(int), typeof(VariablesViewModel), new PropertyMetadata(0));

        public ICommand ClikToSolve
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ProgressStatus++;
                });
            }
        }
    }
}

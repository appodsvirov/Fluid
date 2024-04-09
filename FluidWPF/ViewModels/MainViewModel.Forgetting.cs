using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluidWPF.ViewModels
{
    public partial class MainViewModel: ReactiveObject
    {
        private Thread _thread;
        private CancellationTokenSource _tokenSource;
        public event EventHandler<Variables> _solveRequested;
        [Reactive] public int Dt { get; set; } = 60;
        [Reactive] public double InSpeed { get; set; } = 1;
        [Reactive] public double Rad2 { get; set; } = 0.01;
        [Reactive] public double Height { get; set; } = 0.1;
        [Reactive] public int CountSteps { get; set; } = 2000;
        [Reactive] public int ScaleNet { get; set; } = 1;
        [Reactive] public int Dlog { get; set; } = 1;
        [Reactive] public int Start { get; set; }
        [Reactive] public string Reynolds { get; set; } = 1E5.ToString("E");
        [Reactive] public bool IsSolveTurbulence { get; set; } = false;
        [Reactive] public bool Is4Сylinders { get; set; } = false;
        [Reactive] public bool InProggress { get; set; } = false;
        [Reactive] public bool Saving { get; set; } = false;

        public ReactiveCommand<Unit, Unit> ClikToSolve { get; set; }
        public ReactiveCommand<Unit, Unit> ClikToCopy { get; set; }
        public ReactiveCommand<Unit, Unit> ClikToStop { get; set; }
        public ReactiveCommand<object, Unit> OpenFilePressureCommand { get; set; }
    }
}

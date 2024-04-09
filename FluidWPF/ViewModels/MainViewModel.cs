using FluidWPF.Abstract;
using FluidWPF.Models;
using FluidWPF.Utilities;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FluidWPF.ViewModels
{
    public partial class MainViewModel: ReactiveObject
    {
        /*public VariablesViewModel Variables { get; set; }*/
        public ScenarioManagementViewModel Scenarios { get; set; }
        public MainViewModel() 
        { 
            //Variables = new VariablesViewModel();
            Scenarios = new ScenarioManagementViewModel();
            _solveRequested += Scenarios.ScenarioRequeste;


            //
            ClikToSolve = ReactiveCommand.Create(() =>
            {
                _tokenSource = new CancellationTokenSource();
                _thread = new Thread(Solve) { IsBackground = true };
                _thread.Start(_tokenSource.Token);
            });

            ClikToCopy = ReactiveCommand.Create(() => Clipboard.SetText(Reynolds));

            ClikToStop = ReactiveCommand.Create(() => {
                _tokenSource.Cancel();
                _tokenSource = null;
                _thread = null;
            });

            OpenFilePressureCommand = ReactiveCommand.Create<object>((object obj) =>
            {
                Variables selectedVariables = obj as Variables;
                if (obj == null)
                {
                    return;
                }
                string path = Path.Combine("Data\\Simple", "P" + selectedVariables.Id.ToString() + ".txt");
                if (File.Exists(path))
                {
                    Process.Start("notepad.exe", path);
                }
                Console.WriteLine();
            });

            this.WhenAnyValue(x =>  x.Height).Subscribe( _=> UpdateReynolds());
            this.WhenAnyValue(x =>  x.InSpeed).Subscribe( _=> UpdateReynolds());
        }
    }
}

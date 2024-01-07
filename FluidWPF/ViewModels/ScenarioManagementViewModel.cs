using FluidWPF.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidWPF.ViewModels
{
    public class ScenarioManagementViewModel:BaseVM
    {
        private ObservableCollection<Variables> listVariables = new ObservableCollection<Variables>();
        public ObservableCollection<Variables> ScenarioItems
        {
            get => listVariables;
            set
            {
                listVariables = value;
                OnPropertyChanged();
            }
        }
        public void ScenarioRequeste(object? sender, Variables e)
        {
           switch (e.statusRequeste)
            {
                case Enums.StatusRequeste.Add:
                    Dispatcher.Invoke(() => ScenarioItems.Add(e));
                    break;
                case Enums.StatusRequeste.Update:
                    Dispatcher.Invoke(() => ScenarioItems[e.Id].Completion = e.Completion); break;
            }
            
        }
    }
}

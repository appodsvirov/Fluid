using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidWPF.ViewModels
{
    public class MainViewModel:DependencyObject
    {
        public VariablesViewModel Variables { get; set; }

        public MainViewModel() 
        { 
            Variables = new VariablesViewModel();
        }
    }
}

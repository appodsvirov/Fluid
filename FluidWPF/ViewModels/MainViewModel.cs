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
    public class MainViewModel:BaseVM
    {
        public VariablesViewModel Variables { get; set; }
        public MainViewModel() 
        { 
            Variables = new VariablesViewModel();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluidWPF.Abstract;
using FluidWPF.ViewModels.Enums;

namespace FluidWPF.ViewModels
{
    public class Variables: BaseVM
    {
        public static int count = 0;  
        public Variables(int id) 
        {
            statusRequeste = StatusRequeste.Delete;
            Id = id;
        }
        public Variables(string title)
        {
            statusRequeste = StatusRequeste.Add;
            Title = title;
            Id = count++;
        }
        public Variables(int id, int completion)
        {
            statusRequeste = StatusRequeste.Update;
            Id = id;
            Completion = completion;
        }
        public StatusRequeste statusRequeste { get; set; }
        public int Id { get; set; }
        public string? Title { get; set; }
        private int completion; 
        public int Completion 
        {
            get => completion;
            set
            {
                completion = value;
                OnPropertyChanged();
            }
        }
    }
}

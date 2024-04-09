using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluidWPF.Abstract;
using FluidWPF.ViewModels.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace FluidWPF.ViewModels
{
    public class Variables : ReactiveObject
    {
        public static int count = 0;
        public StatusRequeste statusRequeste { get; set; }
        public int Id { get; set; }
        public string? Title { get; set; }
        [Reactive] public int Completion { get; set; }
        [Reactive] public double Time { get; set; }

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
        public Variables(int id, int completion, double seconds = -1)
        {
            statusRequeste = StatusRequeste.Update;
            Id = id;
            Completion = completion;
            Time = seconds;
        }
    }
}

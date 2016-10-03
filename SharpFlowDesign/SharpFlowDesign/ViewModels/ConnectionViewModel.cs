﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Behavior;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class ConnectionViewModel : INotifyPropertyChanged, IDragable
    {
        public ConnectionViewModel(IOCellViewModel start, IOCellViewModel end)
        {
            Start = start;
            End = end;
            Name = "Parameter";
        }


        public bool IsDragging { get; set; }
        public IOCellViewModel Start { get; set; }
        public IOCellViewModel End { get; set; }
        public string Name { get; set; }
        public Point Center { get; set; }
        public double AngleText { get; set; }


        Type IDragable.DataType => typeof (ConnectionViewModel);

        void IDragable.Remove(object i)
        {
            MainViewModel.Instance().RemoveConnection(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
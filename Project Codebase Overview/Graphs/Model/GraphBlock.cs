using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.Graphs.Model
{
    public class GraphBlock : ObservableObject
    {
        private Color _color;
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private double _percentage;
        public double Percentage { get => _percentage; set => SetProperty(ref _percentage, value); }
        public double StartValue { get; set; }
        public double EndValue { get; set; }
        public string ToolTip { get; set; }
        public bool IsCreator { get; set; }
        public bool IsActive { get; set; }


        public GraphBlock()
        {

        }


    }
}

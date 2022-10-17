using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.Graphs.Model
{
    public class GraphBlock
    {
        public Color Color { get; set; }
        public string Name { get; set; }
        public double Percentage { get; set; }
        public double StartValue { get; set; }
        public double EndValue { get; set; }
        public string ToolTip { get; set; }
        public bool IsCreator { get; set; }


        public GraphBlock()
        {

        }


    }
}

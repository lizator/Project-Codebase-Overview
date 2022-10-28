using Microsoft.UI.Xaml;
using Project_Codebase_Overview.DataCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.Graphs.Model
{
    
    public class DoughnutDataUnit
    {
        public string Name;
        public Color Color;
        public uint LinesTotal;
        public Visibility Visibility;
        public ExplorerItem ExplorerItem;

        public DoughnutDataUnit(string name, uint linesTotal, Color color, Visibility visibility, ExplorerItem explorerItem)
        {
            Name = name;
            Color = color;
            LinesTotal = linesTotal;
            Visibility = visibility;
            ExplorerItem = explorerItem;
        }
    }
}

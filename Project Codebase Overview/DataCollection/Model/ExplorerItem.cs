using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.Graphs;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public abstract class ExplorerItem : IComparable
    {
        private static readonly int BAR_GRAPH_CUT_OFF_POINT = 90;
        private static readonly int BAR_GRAPH_SMALL_CHECK_STARTING_POINT = 60;
        private static readonly int BAR_GRAPH_SMALL_CHECK_MIN_SIZE = 5;
        public string Name { get; set; }
        public abstract void CalculateData();
        public abstract int CompareTo(object obj);

        public GraphModel GraphModel { get; set; }
        public PCOFolder Parent { get; set; }
        public string SuggestedOwnerName { get; }

        public SfLinearGauge BarGraph
        {
            get => GetBarGraph();
        }
        protected SfLinearGauge GetBarGraph()
        {
            SfLinearGauge sfLinearGauge = new SfLinearGauge();
            sfLinearGauge.Axis.Minimum = 0;
            sfLinearGauge.Axis.Maximum = 100;
            foreach (var block in GraphHelper.GetGraphBlocksFromDistribution(this.GraphModel.LineDistribution, this.GraphModel.LinesTotal))
            {
                LinearGaugeRange gaugeRange = new LinearGaugeRange();
                gaugeRange.StartValue = block.StartValue;
                gaugeRange.EndValue = block.EndValue;
                gaugeRange.StartWidth = 25;
                gaugeRange.EndWidth = 25;
                gaugeRange.Background = new SolidColorBrush(block.Color);

                ToolTipService.SetToolTip(gaugeRange, block.ToolTip);

                sfLinearGauge.Axis.Ranges.Add(gaugeRange);
            }
            _bargraph = sfLinearGauge;
            return sfLinearGauge;
        }

        protected SfLinearGauge _bargraph { get; set; }

    }
}

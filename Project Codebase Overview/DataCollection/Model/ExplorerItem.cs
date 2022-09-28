using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
            double currentStartPos = 0;
            var distributionAmount = this.GraphModel.LineDistribution.Count();
            uint lineCount = 0;
            var blockCount = 0;

            foreach (var dist in this.GraphModel.LineDistribution.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value))
            {
                var blockSize = ((double)dist.Value / (double)this.GraphModel.LinesTotal) * 100;
                if (
                    blockCount < distributionAmount - 1 &&  //If there is only one block left, just show its color.
                    (currentStartPos > BAR_GRAPH_CUT_OFF_POINT || // if more blocks after the cut off point, make them one grey block
                    (currentStartPos > BAR_GRAPH_SMALL_CHECK_STARTING_POINT && blockSize < BAR_GRAPH_SMALL_CHECK_MIN_SIZE)) // if the blocks are very small and after the "small_check_starting_point" make them one grey block
                )
                {//Adding "other"-section to graph
                    LinearGaugeRange gaugeRange = new LinearGaugeRange();
                    gaugeRange.StartValue = currentStartPos;
                    gaugeRange.EndValue = 100;
                    gaugeRange.StartWidth = 25;
                    gaugeRange.EndWidth = 25;
                    gaugeRange.Background = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));

                    var msg = string.Format("{0} others: {1:N2}%", distributionAmount-blockCount, (1-(double)lineCount / (double)this.GraphModel.LinesTotal) * 100);

                    ToolTipService.SetToolTip(gaugeRange, msg);

                    currentStartPos = gaugeRange.EndValue;

                    sfLinearGauge.Axis.Ranges.Add(gaugeRange);
                    break;
                } else
                {
                    LinearGaugeRange gaugeRange = new LinearGaugeRange();
                    gaugeRange.StartValue = currentStartPos;
                    gaugeRange.EndValue = blockSize + currentStartPos;
                    gaugeRange.StartWidth = 25;
                    gaugeRange.EndWidth = 25;
                    gaugeRange.Background = new SolidColorBrush(dist.Key.Color);

                    var msg = string.Format("{0}: {1:N2}%", dist.Key.Name, blockSize);

                    ToolTipService.SetToolTip(gaugeRange, msg);

                    currentStartPos = gaugeRange.EndValue;

                    lineCount += dist.Value;

                    sfLinearGauge.Axis.Ranges.Add(gaugeRange);
                }
                blockCount++;
                
            }
            _bargraph = sfLinearGauge;
            return sfLinearGauge;
        }

        protected SfLinearGauge _bargraph { get; set; }

    }
}

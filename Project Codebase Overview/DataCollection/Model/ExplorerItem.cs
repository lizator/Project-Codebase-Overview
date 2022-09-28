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

            foreach (var dist in this.GraphModel.LineDistribution.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value) )
            {

                LinearGaugeRange gaugeRange = new LinearGaugeRange();
                gaugeRange.StartValue = currentStartPos;
                gaugeRange.EndValue = ((double)dist.Value / (double)this.GraphModel.LinesTotal) * 100 + currentStartPos;
                gaugeRange.StartWidth = 25;
                gaugeRange.EndWidth = 25;
                gaugeRange.Background = new SolidColorBrush(dist.Key.Color);

                currentStartPos = gaugeRange.EndValue;

                sfLinearGauge.Axis.Ranges.Add(gaugeRange);
            }
            _bargraph = sfLinearGauge;
            return sfLinearGauge;
        }

        protected SfLinearGauge _bargraph { get; set; }

    }
}

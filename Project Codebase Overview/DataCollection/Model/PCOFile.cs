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
    public class PCOFile : IExplorerItem
    {
        public string Name { get; set; }

        public GraphModel GraphModel { get; set;}

        public PCOFolder Parent { get; set; }

        public List<PCOCommit> commits;

        public void CalculateData()
        {
            //TODO: ask Should this be handled inside graphmodel instead?
            var groupedCommits = this.commits.GroupBy(x => x.GetAuthor());

            foreach (var groupedComm in groupedCommits)
            {
                this.GraphModel.LineDistribution.Add(groupedComm.First().GetAuthor(), 0);
                
                foreach (var commit in groupedComm)
                {
                    this.GraphModel.LinesTotal += (uint) commit.GetLines();
                    this.GraphModel.LineDistribution[commit.GetAuthor()] += (uint) commit.GetLines();  
                }
            }
            this.GraphModel.UpdateSuggestedOwner();
        }

        public int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(PCOFile))
            {
                return 1;
            }
            return string.Compare(this.Name, ((PCOFile)obj).Name, StringComparison.InvariantCulture);
        }

        public string SuggestedOwnerName { get => this.GraphModel.SuggestedOwner?.Name; }

        public PCOFile(string name, PCOFolder parent, List<PCOCommit> commits = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.commits = commits ?? new List<PCOCommit>();
            this.GraphModel = new GraphModel();
            this.GraphModel.FileName = name;
        }

        public SfLinearGauge BarGraph
        {
            get => GetBarGraph();
        }
        private SfLinearGauge GetBarGraph()
        {
            if (_bargraph == null || true )
            {
                SfLinearGauge sfLinearGauge = new SfLinearGauge();
                sfLinearGauge.Axis.Minimum = 0;
                sfLinearGauge.Axis.Maximum = 100;
                double currentStartPos = 0;

                //testing purposes adding colors
                SolidColorBrush[] colors = {
                new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
                new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
                };
                int count = 0;

                foreach (var dist in this.GraphModel.LineDistribution)
                {

                    LinearGaugeRange gaugeRange = new LinearGaugeRange();
                    gaugeRange.StartValue = currentStartPos;
                    gaugeRange.EndValue = ((double)dist.Value / (double)this.GraphModel.LinesTotal) * 100 + currentStartPos;
                    gaugeRange.StartWidth = 25;
                    gaugeRange.EndWidth = 25;
                    gaugeRange.Background = colors[count % 3];
                    count++;

                    currentStartPos = gaugeRange.EndValue;

                    sfLinearGauge.Axis.Ranges.Add(gaugeRange);
                }
                _bargraph = sfLinearGauge;
            }
            return _bargraph;
        }

        private SfLinearGauge _bargraph { get; set; }

    }
}

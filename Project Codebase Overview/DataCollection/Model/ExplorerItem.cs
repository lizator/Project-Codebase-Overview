using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.Graphs;
using Syncfusion.UI.Xaml.Data;
using Project_Codebase_Overview.Graphs.Model;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public abstract class ExplorerItem : ObservableObject, IComparable
    {
        public string Name { get; set; }
        public abstract void CalculateData();
        public abstract int CompareTo(object obj);

        public GraphModel GraphModel { get; set; }
        public PCOFolder Parent { get; set; }
        public string SuggestedOwnerName { get => this.GraphModel.SuggestedOwner?.Name ?? "Undefined"; }
        public SolidColorBrush SuggestedOwnerColor { get => new SolidColorBrush(this.GraphModel.SuggestedOwner?.Color ?? PCOColorPicker.Black); }

        public string SelectedOwnerName { get => this.GraphModel.SelectedOwner?.Name ?? "Unselected"; }
        public SolidColorBrush SelectedOwnerColor { get => new SolidColorBrush(this.GraphModel.SelectedOwner?.Color ?? PCOColorPicker.Black); set => SetProperty(ref selectedOwnerColor, new SolidColorBrush(this.GraphModel.SelectedOwner?.Color ?? PCOColorPicker.Black)); }
        private SolidColorBrush selectedOwnerColor;
        public string LinesTotalString { get; }

        public ObservableCollection<IOwner> Owners { get => ContributorManager.GetInstance().GetAllAuthors().ConvertAll(x => (IOwner)x).ToObservableCollection(); }

        public SfLinearGauge BarGraph
        {
            get => GetBarGraph();
        }
        protected SfLinearGauge GetBarGraph()
        {
            SfLinearGauge sfLinearGauge = new SfLinearGauge();
            sfLinearGauge.Axis.Minimum = 0;
            sfLinearGauge.Axis.Maximum = 101;
            sfLinearGauge.Axis.ShowLabels = false;
            sfLinearGauge.Axis.ShowTicks = false;
            sfLinearGauge.Axis.AxisLineStrokeThickness = 25;
            sfLinearGauge.Axis.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            sfLinearGauge.Axis.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            sfLinearGauge.Axis.Margin = new Microsoft.UI.Xaml.Thickness(1,2,1,2);
            sfLinearGauge.Axis.AxisLineStroke = new SolidColorBrush(Color.FromArgb(255,0,0,0));

            var blocks = new List<GraphBlock>();
            if (this.GetType() == typeof(PCOFile))
            {
                blocks = GraphHelper.GetGraphBlocksFromDistribution(this.GraphModel.LineDistribution, this.GraphModel.LinesTotal, ((PCOFile)this).Creator);
            } else
            {
                blocks = GraphHelper.GetGraphBlocksFromDistribution(this.GraphModel.LineDistribution, this.GraphModel.LinesTotal);
            }


            foreach (var block in blocks)
            {
                LinearGaugeRange gaugeRange = new LinearGaugeRange();
                gaugeRange.StartValue = block.StartValue + 0.5;
                gaugeRange.EndValue = block.EndValue + 0.5;
                gaugeRange.StartWidth = 22;
                gaugeRange.EndWidth = 22;
                gaugeRange.RangePosition = GaugeElementPosition.Cross;
                
                gaugeRange.Background = new SolidColorBrush(block.Color);

                if (block.IsCreator && block.EndValue - block.StartValue > 10)
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri("ms-appx:///Assets/star.png"));
                    img.Height = 20;
                    img.Width = 20;
                    img.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                    gaugeRange.Child = img;
                }

                var tooltip = new ToolTip();
                var tooltipMsg = new TextBlock();
                tooltipMsg.Text = block.ToolTip;
                tooltipMsg.HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center;
                tooltip.Content = tooltipMsg;

                ToolTipService.SetToolTip(gaugeRange, tooltip);

                sfLinearGauge.Axis.Ranges.Add(gaugeRange);
            }
            _bargraph = sfLinearGauge;
            return sfLinearGauge;
        }

        protected SfLinearGauge _bargraph { get; set; }

    }
}

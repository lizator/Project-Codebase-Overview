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
using Project_Codebase_Overview.State;
using System.Globalization;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public abstract class ExplorerItem : ObservableObject, IComparable
    {
        public string Name { get; set; }
        public uint LinesTotalNumber { get => this.GraphModel.LinesTotal; }
        public uint LinesAfterDecayNumber { get => this.GraphModel.LinesAfterDecay; }
        public abstract void CalculateData();
        public abstract int CompareTo(object obj);

        public GraphModel GraphModel { get; set; }
        public PCOFolder Parent { get; set; }
        public string SuggestedOwnerName { get => this.GraphModel.SuggestedOwner?.Name ?? "Undefined"; }
        public SolidColorBrush SuggestedOwnerColor { get => new SolidColorBrush(this.GraphModel.SuggestedOwner?.Color ?? PCOColorPicker.Tranparent); }

        public string SelectedOwnerName { get => this.SelectedOwner?.Name ?? "Unselected"; set => SetProperty(ref selectedOwnerName, this.SelectedOwner?.Name ?? "Unselected"); }
        private string selectedOwnerName;
        public SolidColorBrush SelectedOwnerColor { get => new SolidColorBrush(this.SelectedOwner?.Color ?? PCOColorPicker.Tranparent); set => SetProperty(ref selectedOwnerColor, new SolidColorBrush(this.SelectedOwner?.Color ?? PCOColorPicker.Black)); }
        private SolidColorBrush selectedOwnerColor;

        public ObservableCollection<IOwner> Owners { get => this.GetOwnerListSorted(); }

        public IOwner SelectedOwner { get => _selectedOwner; set => SetProperty(ref _selectedOwner, value); }
        private IOwner _selectedOwner;
        public SfLinearGauge BarGraph
        {
            get => GetBarGraph();
        }

        private ObservableCollection<IOwner> GetOwnerListSorted()
        {
            //create "Unselected" entry
            var ownerlist = PCOState.GetInstance().GetContributorState().GetAllOwners().OrderBy(x => x.Name).ToList();
            if (this.GraphModel.SuggestedOwner != null)
            {
                var ownerIndex = ownerlist.IndexOf(this.GraphModel.SuggestedOwner);
                if(ownerIndex > -1)
                {
                    ownerlist.MoveTo(ownerlist.IndexOf(this.GraphModel.SuggestedOwner), 0);
                }
            }
            if (PCOState.GetInstance().GetSettingsState().CurrentMode == Settings.PCOExplorerMode.USER)
            {
                ownerlist = ownerlist.Where(author => ((Author)author).IsActive).ToList();
            }
            ownerlist.Add(new Author("Unselected", "Unselected"));
            return ownerlist.ToObservableCollection();
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
            if (this.GraphModel.LinesTotal > 0)
            {
                if (this.GetType() == typeof(PCOFile))
                {
                    blocks = GraphHelper.GetGraphBlocksFromDistribution(this.GraphModel, ((PCOFile)this).Creator);
                }
                else
                {
                    blocks = GraphHelper.GetGraphBlocksFromDistribution(this.GraphModel);
                }
            } else
            {
                this.GraphModel.SuggestedOwner = null;
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

                if (block.EndValue - block.StartValue > 10)
                {
                    var panel = new StackPanel();
                    panel.Orientation = Orientation.Horizontal;
                    panel.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;

                    if (block.IsCreator)
                    {
                        Image img = new Image();
                        img.Source = new BitmapImage(new Uri("ms-appx:///Assets/star.png"));
                        img.Height = 20;
                        img.Width = 20;
                        img.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                        panel.Children.Add(img);
                    }

                    if (!block.IsActive)
                    {
                        Image img = new Image();
                        img.Source = new BitmapImage(new Uri("ms-appx:///Assets/noBW.png"));
                        img.Height = 20;
                        img.Width = 20;
                        img.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                        panel.Children.Add(img);
                    }

                    gaugeRange.Child = panel;
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

        public void GenerateBarGraph()
        {
            this.GetBarGraph();
        }

        public string GetRelativePath()
        {
            PCOFolder tempFolder = this.Parent;
            string relativePath = "";
            if (this is PCOFolder folder)
            {
                tempFolder = folder;
            } else
            {
                relativePath = this.Name;
            }
            while (tempFolder?.Parent != null)
            {
                relativePath = tempFolder.Name + "\\" + relativePath;
                tempFolder = tempFolder.Parent;
            }
            return relativePath;
        }
        
    }
}

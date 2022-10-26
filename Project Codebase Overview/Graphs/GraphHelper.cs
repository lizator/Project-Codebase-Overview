using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Graphs.Model;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Charts;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.Graphs
{
    public class GraphHelper
    {

        private static readonly int GRAPH_CUT_OFF_POINT = 90;
        private static readonly int GRAPH_SMALL_CHECK_STARTING_POINT = 60;
        private static readonly int GRAPH_SMALL_CHECK_MIN_SIZE = 5;

        public static List<GraphBlock> GetGraphBlocksFromDistribution(Dictionary<Author, uint> distributions, uint linesTotal, Author creator = null)
        {
            //Creator is only given for files

            var blockList = new List<GraphBlock>();
            double currentStartPos = 0;
            var distributionAmount = distributions.Count();
            uint lineCount = 0;
            var blockCount = 0;

            foreach (var dist in distributions.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value))
            {
                var blockSize = ((double)dist.Value / (double)linesTotal) * 100;
                if (
                    blockCount < distributionAmount - 1 &&  //If there is only one block left, just show its color.
                    (currentStartPos > GRAPH_CUT_OFF_POINT || // if more blocks after the cut off point, make them one grey block
                    (currentStartPos > GRAPH_SMALL_CHECK_STARTING_POINT && blockSize < GRAPH_SMALL_CHECK_MIN_SIZE)) // if the blocks are very small and after the "small_check_starting_point" make them one grey block
                )
                {//Adding "other"-section to graph
                    GraphBlock block = new GraphBlock();
                    block.StartValue = currentStartPos;
                    block.EndValue = 100;
                    block.Color = Color.FromArgb(255, 150, 150, 150);
                    block.Name = String.Format("{0} others", distributionAmount - blockCount);
                    block.Percentage = (1 - (double)lineCount / (double)linesTotal) * 100;
                    block.ToolTip = string.Format("{0} others: {1:N2}%", distributionAmount - blockCount, (1 - (double)lineCount / (double)linesTotal) * 100);

                    currentStartPos = block.EndValue;

                    blockList.Add(block);
                    break;
                }
                else
                {
                    GraphBlock block = new GraphBlock();
                    block.StartValue = currentStartPos;
                    block.EndValue = blockSize + currentStartPos;
                    block.Color = dist.Key.Color;
                    block.Name = dist.Key.Name;
                    block.Percentage = blockSize;

                    block.IsCreator = creator != null && dist.Key.Email == creator.Email;

                    if (block.IsCreator)
                    {
                        block.ToolTip = string.Format("{0}: {1:N2}%\nCreator", dist.Key.Name, blockSize);
                    } else
                    {
                        block.ToolTip = string.Format("{0}: {1:N2}%", dist.Key.Name, blockSize);
                    }



                    currentStartPos = block.EndValue;

                    lineCount += dist.Value;

                    blockList.Add(block);
                }
                blockCount++;

            }

            return blockList;
        }

        public static SfCircularChart GetPieChartFromExplorerItem(ExplorerItem item)
        {
            List<GraphBlock> blocks = GraphHelper.GetGraphBlocksFromDistribution(item.GraphModel.LineDistribution, item.GraphModel.LinesTotal, null);

            //piechart for linedistribution between contributors
            SfCircularChart lineDistChart = new SfCircularChart();
            lineDistChart.Header = "Contributor line distribution";
            lineDistChart.Legend = new ChartLegend();

            //make color pallette for series
            var colorPallette = blocks.Select(x => new SolidColorBrush(x.Color) as Brush).ToList();
            //create series (pie slices)
            var lineDistSeries = new PieSeries();
            lineDistSeries.ItemsSource = blocks;
            lineDistSeries.XBindingPath = "Name";
            lineDistSeries.YBindingPath = "Percentage";
            lineDistSeries.ShowDataLabels = true;
            lineDistSeries.DataLabelSettings = new CircularDataLabelSettings()
            {
                Position = CircularSeriesLabelPosition.OutsideExtended,
                ShowConnectorLine = true,
                Context = LabelContext.Percentage,
                //TODO: make template in XAML for label to show name and percentage!
            };

            lineDistSeries.PaletteBrushes = colorPallette;
            lineDistSeries.EnableAnimation = true;

            lineDistChart.Series.Add(lineDistSeries);

            return lineDistChart;
        }

        internal static object GetCurrentSunburst(DataTemplate tooltipTemplate)
        {
            Grid grid = new Grid();
            SfCircularChart circularChart = new SfCircularChart();
            ChartTooltipBehavior tooltipBehavior = new ChartTooltipBehavior();
            tooltipBehavior.EnableAnimation = false;
            tooltipBehavior.ShowDuration = 8000;

            circularChart.TooltipBehavior = tooltipBehavior;

            PCOFolder rootFolder = PCOState.GetInstance().GetExplorerState().GetCurrentRootFolder();

            int maxDepth = 6;
            //initialize lists of data
            List<List<DoughnutDataUnit>> dataLists = new List<List<DoughnutDataUnit>>();
            for(int i = 0; i < maxDepth; i++)
            {
                dataLists.Add(new List<DoughnutDataUnit>());
            }

            foreach(var child in rootFolder.Children.Values)
            {
                GetDoughnutDataLists(0, maxDepth, child, dataLists);
            }

            foreach(List<DoughnutDataUnit> list in dataLists)
            {
                DoughnutSeries series = new DoughnutSeries();
                //series.ItemsSource = rootFolder.Children.Values.Select(x => new {Name=x.Name, LinesTotal=x.GraphModel.LinesTotal});
                series.ItemsSource = list.Select(x => new { Name = x.Name, LinesTotal = x.LinesTotal, Visibility = x.Visibility});
                series.XBindingPath = "Name";
                series.YBindingPath = "LinesTotal";
                series.Radius = 1;
                series.InnerRadius = 0.05;
                series.EnableTooltip = true;
                series.TooltipTemplate = tooltipTemplate;
                

                series.PaletteBrushes = list.Select(x => new SolidColorBrush(x.Color) as Brush).ToList();
                series.Stroke = new SolidColorBrush(Colors.White);

                circularChart.Series.Add(series);
            }

            grid.Children.Add(circularChart);
            return grid;
            
        }

        class DoughnutDataUnit
        {
            public string Name;
            public Color Color;
            public uint LinesTotal;
            public Visibility Visibility;
            public DoughnutDataUnit(string name, uint linesTotal, Color color, Visibility visibility)
            {
                Name = name;
                Color = color;
                LinesTotal = linesTotal;
                Visibility = visibility;
            }
        }

        //get the name and lines data for each level in the graph
        private static void GetDoughnutDataLists(int depth, int maxDepth, ExplorerItem explorerItem, List<List<DoughnutDataUnit>> dataLists)
        {
            //add data from current explorer item
            if (depth == maxDepth)
            {
                return;
            }

            dataLists[depth].Add(
                        new DoughnutDataUnit(explorerItem.Name, explorerItem.GraphModel.LinesTotal, 
                        explorerItem.GraphModel.SuggestedOwner.Color, Visibility.Visible));

            if (explorerItem.GetType() == typeof(PCOFile))
            {
                //file
                //create whitespace outwards
                for(int i = depth+1; i < maxDepth; i++)
                {
                    dataLists[i].Add( new DoughnutDataUnit("", explorerItem.GraphModel.LinesTotal, Colors.White, Visibility.Collapsed));
                }
            }
            else
            {
                //folder
                //call for all children
                foreach(var child in ((PCOFolder)explorerItem).Children.Values)
                {
                    GetDoughnutDataLists(depth + 1, maxDepth, child, dataLists);
                }
            }
            
        }
        

        
    }
}

﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Graphs.Model;
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
    }
}

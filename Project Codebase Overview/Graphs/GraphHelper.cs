using LibGit2Sharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Graphs.Model;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Charts;
using Syncfusion.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
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

        internal static Grid GetCurrentTreeGraph()
        {
            Grid grid = new Grid();
            grid.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
            grid.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

            Path path = new Path();
            path.Stroke = new SolidColorBrush(Microsoft.UI.Colors.Black);
            path.StrokeThickness = 1;

            Point center = new Point(100, 100);
            int radiusInterval = 30;

            var geometryGroup1 = new GeometryGroup();

            var ellipseGeometry1 = new EllipseGeometry();
            ellipseGeometry1.Center = center;
            ellipseGeometry1.RadiusX = radiusInterval;
            ellipseGeometry1.RadiusY = radiusInterval;
            geometryGroup1.Children.Add(ellipseGeometry1);

            var ellipseGeometry2 = new EllipseGeometry();
            ellipseGeometry2.Center = center;
            ellipseGeometry2.RadiusX = radiusInterval * 2;
            ellipseGeometry2.RadiusY = radiusInterval * 2;
            geometryGroup1.Children.Add(ellipseGeometry2);

            var ellipseGeometry3 = new EllipseGeometry();
            ellipseGeometry3.Center = center;
            ellipseGeometry3.RadiusX = radiusInterval * 3;
            ellipseGeometry3.RadiusY = radiusInterval * 3;
            geometryGroup1.Children.Add(ellipseGeometry3);

            PCOFolder rootFolder = PCOState.GetInstance().GetExplorerState().GetCurrentRootFolder();

            int maxDepth = 3;
            int leafNodeTotal = GetLeafNodeTotal(0, maxDepth, rootFolder);


            double interval = 6.28 / leafNodeTotal;

            path.Data = geometryGroup1;

            grid.Children.Add(path);

            SetExplorerItemsXY(radiusInterval, 0, rootFolder, interval, 0, maxDepth);

            var lines = AddPathsRecursively(rootFolder);

            var transform = new TranslateTransform();
            transform.X = center.X;
            transform.Y = center.Y;
            foreach(var line in lines)
            {
                line.RenderTransform = transform;
                grid.Children.Add(line);
            }

            return grid;
        }

        private static int GetLeafNodeTotal(int depth, int maxDepth, ExplorerItem explorerItem)
        {
            int leafNodeCount = 0;
            if(explorerItem.GetType() == typeof(PCOFile) || depth == maxDepth)
            {
                return 1;
            }
            else
            {
                foreach(var child in ((PCOFolder)explorerItem).Children.Values)
                {
                    leafNodeCount += GetLeafNodeTotal(depth + 1, maxDepth, child);
                }
            }
            return leafNodeCount;
        }

        private static int SetExplorerItemsXY(int radiusInterval, int d, ExplorerItem explorerItem, double interval, int leafNodeCounter, int maxDepth)
        {
            int radius = radiusInterval * d;

            if(d == maxDepth || explorerItem.GetType() == typeof(PCOFile))
            {
                //edge nodes or files
                double angle = interval * leafNodeCounter;
                explorerItem.GraphModel.x = Math.Cos(angle) * radius;
                explorerItem.GraphModel.y = Math.Sin(angle) * radius;
                leafNodeCounter++;
            }
            else
            {
                //non edge folders
                double childMinAngle = leafNodeCounter * interval;
                foreach (var child in ((PCOFolder)explorerItem).Children.Values)
                {
                    leafNodeCounter = SetExplorerItemsXY(radiusInterval, d + 1, child, interval, leafNodeCounter, maxDepth);
                }
                double childMaxAngle = leafNodeCounter * interval; //leafnodecounter changed after recursing children
                double angle = (childMaxAngle + childMinAngle) / 2;
                explorerItem.GraphModel.x = Math.Cos(angle) * radius;
                explorerItem.GraphModel.y = Math.Sin(angle) * radius;

            }
            return leafNodeCounter;
        }

        

        private static List<Line> AddPathsRecursively(PCOFolder folder)
        {
            List<Line> lines = new List<Line>();

         
            foreach(var child in folder.Children.Values)
            {
                Line line = new Line();
                line.Stroke = child.SuggestedOwnerColor;
                line.StrokeThickness = 2;
                //startpoint
                line.X1 = folder.GraphModel.x;
                line.Y1 = folder.GraphModel.y;
                //endpoint
                line.X2 = child.GraphModel.x;
                line.Y2 = child.GraphModel.y;
                lines.Add(line);
                if(child.GetType() == typeof(PCOFolder))
                {
                    lines.AddRange(AddPathsRecursively((PCOFolder)child));
                }
            }
            return lines;
        }


    }
}

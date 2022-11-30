using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Graphs;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.DataGrid;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.NumberFormatting;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview.Dialogs
{
    public class TopContributor
    {
        public string Name { get; set; }
        public string Team { get; set; }
        public double Lines { get; set; }
        public double Percent { get; set; }
        public TopContributor(string name, string team, double lines, double percent)
        {
            Name = name;
            Team = team;
            Lines = lines;
            Percent = percent;
        }
    }
    public class ExpandDialogParameters
    {
        public ExplorerItem Item;
        public ContentDialog Dialog;
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExpandExplorerItemDialog : Page
    {
        public ObservableCollection<TopContributor> TopContributors;
        private ContentDialog DialogRef;
        private ExplorerItem ExplorerItem;
        private string ItemPath;

        public ExpandExplorerItemDialog()
        {
            this.InitializeComponent();
            TopContributors = new ObservableCollection<TopContributor>(); 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            ExpandDialogParameters parameters = e.Parameter as ExpandDialogParameters;
            ExplorerItem = parameters.Item;
            DialogRef = parameters.Dialog;

            CommentBox.Text = ExplorerItem.Comment ?? "";

            //set Top Contributors
            uint totalLines = ExplorerItem.GraphModel.LinesTotal;
            if(PCOState.GetInstance().GetSettingsState().CurrentMode == Settings.PCOExplorerMode.USER)
            {
                DataGrid.Columns.Add(new GridTextColumn() { MappingName = "Name", 
                    TextTrimming = TextTrimming.CharacterEllipsis});

                DataGrid.Columns.Add(new GridTextColumn() { MappingName = "Team", 
                    TextTrimming = TextTrimming.CharacterEllipsis });

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Lines",
                    ColumnWidthMode = Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells });

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Percent", HeaderText = "%", 
                    DisplayNumberFormat="P0", ColumnWidthMode=Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells }) ;
                foreach (var dist in ExplorerItem.GraphModel.LineDistribution)
                {
                    Author author = dist.Key as Author;
                    string teamName = author.Team?.Name ?? "";
                    TopContributors.Add(new TopContributor(author.Name, teamName, dist.Value,(double) dist.Value / (double) totalLines));
                }
            }
            else
            {
                DataGrid.Columns.Add(new GridTextColumn() { MappingName = "Team", 
                    TextTrimming = TextTrimming.CharacterEllipsis });

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Lines",
                    ColumnWidthMode = Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells });

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Percent", HeaderText = "%", 
                    DisplayNumberFormat="P0", ColumnWidthMode=Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells }) ;
                foreach (var dist in ExplorerItem.GraphModel.LineDistribution)
                {
                    PCOTeam team = dist.Key as PCOTeam;
                    TopContributors.Add(new TopContributor("", team.Name, dist.Value, dist.Value / totalLines));
                }
            }
            

            TeamDistPie.Content = GraphHelper.GetPieChartFromExplorerItem(ExplorerItem);
        }

        private void CommentChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            ExplorerItem.Comment = CommentBox.Text;
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            DialogRef.Hide();
        }
    }
}

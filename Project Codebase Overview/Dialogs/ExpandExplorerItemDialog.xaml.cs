using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.ChangeHistoryFolder;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Graphs;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Data;
using Syncfusion.UI.Xaml.DataGrid;
using Syncfusion.UI.Xaml.Editors;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public string Teams { get; set; }
        public double Lines { get; set; }
        public double Percent { get; set; }
        public TopContributor(string name, string teams, double lines, double percent)
        {
            Name = name;
            Teams = teams;
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
        public ObservableCollection<IOwner> OwnerList;
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

            //set ownerlist
            OwnerList = PCOState.GetInstance().GetContributorState().GetAllOwnersInMode().ToObservableCollection();


            //set path
            string path = PCOState.GetInstance().GetExplorerState().GetRootPath().Replace('\\', '/')
                + "/" + ExplorerItem.GetRelativePath(true);
            if (path.Length > 70)
            {
                path = "..." + path.Substring(path.Length - 67);
            }
            ItemPath = path;

            //set comment
            CommentBox.Text = ExplorerItem.Comment ?? "";

            //set Top Contributors
            uint totalLines = ExplorerItem.GraphModel.LinesTotal;
            if(PCOState.GetInstance().GetSettingsState().CurrentMode == Settings.PCOExplorerMode.AUTHOR)
            {
                //author mode
                DataGrid.Columns.Add(new GridTextColumn() { MappingName = "Name", 
                    TextTrimming = TextTrimming.CharacterEllipsis,
                ShowToolTip = true});

                DataGrid.Columns.Add(new GridTextColumn() { MappingName = "Teams", 
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HeaderText="Team(s)",
                    ShowToolTip = true});

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Lines",
                    ColumnWidthMode = Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells });

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Percent", HeaderText = "%", 
                    DisplayNumberFormat="P0", ColumnWidthMode=Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells }) ;

                
                foreach (var dist in ExplorerItem.GraphModel.LineDistribution.OrderByDescending(x => x.Value.LineSum()))
                {
                    Author author = dist.Key as Author;
                    string teamName = "";
                    if (author.Teams.Count > 1)
                    {
                        teamName = "Multiple";
                    }
                    else if(author.Teams.Count > 0)
                    {
                        teamName = author.Teams[0].Name;
                    }
                    
                    TopContributors.Add(new TopContributor(author.Name, teamName, dist.Value.LineSum(),(double) dist.Value.LineSum() / (double) totalLines));
                }
                
            }
            else
            {
                //Teams mode
                DataGrid.Columns.Add(new GridTextColumn() { MappingName = "Name", 
                    TextTrimming = TextTrimming.CharacterEllipsis ,
                    HeaderText="Team name"});

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Lines",
                    ColumnWidthMode = Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells });

                DataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Percent", HeaderText = "%", 
                    DisplayNumberFormat="P0", ColumnWidthMode=Syncfusion.UI.Xaml.Grids.ColumnWidthMode.SizeToCells }) ;
                foreach (var dist in ExplorerItem.GraphModel.LineDistribution.OrderByDescending(x => x.Value.LineSum()))
                {
                    PCOTeam team = dist.Key as PCOTeam;
                    TopContributors.Add(new TopContributor(team.Name, "", dist.Value.LineSum(), (double) dist.Value.LineSum() / (double) totalLines));
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

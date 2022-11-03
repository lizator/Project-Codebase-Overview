using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Core;
using Project_Codebase_Overview.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManagementPage : Page
    {
        private ObservableCollection<PCOTeam> Teams;
        public ManagementPage()
        {
            this.InitializeComponent();

            List<Author> owners = new List<Author>();

            //Teams dummy data for visuals
            var memberList = ContributorManager.GetInstance().GetAllAuthors();
            var member = memberList.First();

            for(int i=0; i<14; i++)
            {
                owners.Add(member);
            }
       

            Teams = new ObservableCollection<PCOTeam>();
            Teams.Add(new PCOTeam("Team Field Planning", PCOColorPicker.HardcodedColors[0], owners.GetRange(0,1)));
            Teams.Add(new PCOTeam("Team Field On Site", PCOColorPicker.HardcodedColors[1], owners.GetRange(0,6)));
            Teams.Add(new PCOTeam("Team FM", PCOColorPicker.HardcodedColors[2], owners.GetRange(0,7 )));
            Teams.Add(new PCOTeam("Team Insight", PCOColorPicker.HardcodedColors[3], owners.GetRange(0, 8)));
            Teams.Add(new PCOTeam("Team Box Web", PCOColorPicker.HardcodedColors[4], owners.GetRange(0, 9)));
            Teams.Add(new PCOTeam("Team Box IO", PCOColorPicker.HardcodedColors[5], owners.GetRange(0, 10)));
            
        }

        private void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine("imageFailed: Exception: " + e.ErrorMessage);
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            ((Application.Current as App)?.MainWindow as MainWindow).NavigateToExplorerPage();
        }

        private void TeamClicked(object sender, ItemClickEventArgs e)
        {

        }

        private async void AddTeamClick(object sender, RoutedEventArgs e)
        {
            await DialogHandler.ShowAddEditTeamDialog(this.XamlRoot);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //team item loaded in grid - update the "+X more members" textblock 
            var grid = sender as Grid;
            var team = grid.DataContext as PCOTeam;

            TextBlock textblock = grid.FindChildByName("MoreTextBlock") as TextBlock;
            TextBlock memberListTextBlock= grid.FindChildByName("MemberList") as TextBlock;

            int shownItemCount;
            int maxShownItemsWOMore = 8;

            if(team.Members.Count() > maxShownItemsWOMore)
            {
                shownItemCount = maxShownItemsWOMore - 1;
                textblock.Text = "+" + (team.Members.Count() - shownItemCount) + " more";
                textblock.Visibility = Visibility.Visible;
            }
            else
            {
                shownItemCount = team.Members.Count();
                textblock.Visibility = Visibility.Collapsed;
            }
            StringBuilder stringBuilder = new StringBuilder();
            var visibleMembers = team.Members.GetRange(0, shownItemCount);
            foreach(var member in visibleMembers)
            {
                stringBuilder.AppendLine(member.Name);
            }

            //whitespace
            if(shownItemCount == team.Members.Count())
            {
                int whitespaceCount = maxShownItemsWOMore - shownItemCount;
                for(int i = 0; i < whitespaceCount; i++)
                {
                    stringBuilder.AppendLine(" ");
                }
            }
            memberListTextBlock.Text = stringBuilder.ToString().Substring(0, stringBuilder.Length - 2);

        }
    }
}

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
using Project_Codebase_Overview.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

            

            //Teams dummy data for visuals
            var memberList = ContributorManager.GetInstance().GetAllAuthors();
            Teams = new ObservableCollection<PCOTeam>();
            Teams.Add(new PCOTeam("Team Field Planning", PCOColorPicker.HardcodedColors[0], memberList));
            Teams.Add(new PCOTeam("Team Field On Site", PCOColorPicker.HardcodedColors[1], memberList));
            Teams.Add(new PCOTeam("Team FM", PCOColorPicker.HardcodedColors[2], memberList));
            Teams.Add(new PCOTeam("Team Insight", PCOColorPicker.HardcodedColors[3], memberList));
            Teams.Add(new PCOTeam("Team Box Web", PCOColorPicker.HardcodedColors[4], memberList));
            Teams.Add(new PCOTeam("Team Box IO", PCOColorPicker.HardcodedColors[5], memberList));
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
    }
}

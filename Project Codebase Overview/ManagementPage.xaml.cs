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
        private ObservableCollection<Author> Users;
        public ManagementPage()
        {
            this.InitializeComponent();
       
            Teams = new ObservableCollection<PCOTeam>();
            Users = new ObservableCollection<Author>();


            //DUMMY
            if (false)
            {
                SetDummyUsers();
            } else
            {
                //Not dummy
                var authorList = PCOState.GetInstance().GetContributorState().GetAllAuthors();

                foreach (var author in authorList)
                {
                    Users.Add(author);
                }
            }
            UpdateTeams();
        }
        
        private void SetDummyUsers()
        {
            Users = new ObservableCollection<Author>();
            PCOTeam team = new PCOTeam("SuperTeam", PCOColorPicker.HardcodedColors[0]);
            var authorList = PCOState.GetInstance().GetContributorState().GetAllAuthors();
            var parentAuth = authorList.First();

            foreach(var author in authorList)
            {
                author.ConnectToTeam(team);
                if (!parentAuth.ContainsEmail(author.Email))
                {
                    parentAuth.ConnectAuthor(author);
                }
                if (author.OverAuthor == null) {
                    Users.Add(author);
                }
            }
            PCOState.GetInstance().GetContributorState().AddTeam(team);
            PCOColorPicker.GetInstance();
        }

        private void UpdateTeams()
        {
            Teams.Clear();
            var manager = PCOState.GetInstance().GetContributorState();
            foreach (var team in manager.GetAllTeams())
            {
                Teams.Add(team);
            }
            if(Teams.Count() == 0)
            {
                TeamsGridView.Visibility = Visibility.Collapsed;
                NoTeamsMessage.Visibility = Visibility.Visible;
            }
            else
            {
                TeamsGridView.Visibility = Visibility.Visible;
                NoTeamsMessage.Visibility = Visibility.Collapsed;
            }
            PCOState.GetInstance().GetExplorerState().CalculateData();
        }

        private void UpdateAuthors()
        {
            Users.Clear();
            var manager = PCOState.GetInstance().GetContributorState();
            foreach (var author in manager.GetAllAuthors())
            {
                Users.Add(author);
            }
            PCOState.GetInstance().GetExplorerState().CalculateData();
        }

        private void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine("imageFailed: Exception: " + e.ErrorMessage);
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            ((Application.Current as App)?.MainWindow as MainWindow).NavigateToExplorerPage();
        }

        private async void TeamClicked(object sender, ItemClickEventArgs e)
        {
            var team = e.ClickedItem as PCOTeam;
            await DialogHandler.ShowAddEditTeamDialog(this.XamlRoot, team);

            CheckTeamChangeAndStartUpdate();
        }

        private async void AddTeamClick(object sender, RoutedEventArgs e)
        {
            await DialogHandler.ShowAddEditTeamDialog(this.XamlRoot);

            CheckTeamChangeAndStartUpdate();
        }
        private void CheckTeamChangeAndStartUpdate()
        {
            var manager = PCOState.GetInstance().GetContributorState();
            if (manager.GetTeamUpdated())
            {
                manager.SetTeamUpdated(false);
                UpdateTeams();
            }
        }
        private void CheckAuthorChangeAndStartUpdate()
        {
            var manager = PCOState.GetInstance().GetContributorState();
            if (manager.GetAuthorUpdated())
            {
                manager.SetAuthorUpdated(false);
                UpdateAuthors();
            }
        }

        private async void EditUserClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var author = button.DataContext as Author;
            await DialogHandler.ShowEditAuthorDialog(XamlRoot, author);

            CheckAuthorChangeAndStartUpdate();
        }
    }
}

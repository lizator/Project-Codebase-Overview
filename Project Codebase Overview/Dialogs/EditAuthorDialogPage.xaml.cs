using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection.Emit;
using Microsoft.VisualBasic;
using Project_Codebase_Overview.State;
using Project_Codebase_Overview.Settings;
using Windows.UI;
using Syncfusion.UI.Xaml.Data;
using System.Reflection.Metadata;
using Syncfusion.UI.Xaml.DataGrid;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditAuthorDialogPage : Page
    {
        public ObservableCollection<string> Aliases;
        public ObservableCollection<string> Emails;
        public List<string> AliasList;
        public List<string> EmailList;
        public ObservableCollection<Author> SubAuthors;
        public ObservableCollection<Author> OtherAuthors;
        public ObservableCollection<PCOTeam> Teams;
        public Author CurrentAuthor;
        private bool IsInitialized = false;
        private bool IsActive = true;
        private class Observables : ObservableObject
        {
            private SolidColorBrush _brush;
            public SolidColorBrush Brush { get => _brush; set => SetProperty(ref _brush, value); }
            private string _nameFlyoutMsg;
            public string NameFlyoutMsg { get => _nameFlyoutMsg; set => SetProperty(ref _nameFlyoutMsg, value); }
            private string _emailFlyoutMsg;
            public string EmailFlyoutMsg { get => _emailFlyoutMsg; set => SetProperty(ref _emailFlyoutMsg, value); }
            private string _teamNameMsg;
            public string TeamNameMsg { get => _teamNameMsg; set => SetProperty(ref _teamNameMsg, value); }
            private ObservableCollection<PCOTeam> _selectedTeams;
            public ObservableCollection<PCOTeam> SelectedTeams { get => _selectedTeams; set => SetProperty(ref _selectedTeams, value); }
            private string _selectedTeamsString;
            public string SelectedTeamsString { get => _selectedTeamsString; set => SetProperty(ref _selectedTeamsString, value); }
        }
        private Observables LocalObservables;
        public EditAuthorDialogPage()
        {
            this.InitializeComponent();
            Aliases = new ObservableCollection<string>();
            Emails = new ObservableCollection<string>();
            SubAuthors = new ObservableCollection<Author>();
            OtherAuthors = new ObservableCollection<Author>();
            Teams = new ObservableCollection<PCOTeam>();
            LocalObservables = new Observables();

            var manager = PCOState.GetInstance().GetContributorState();
            CurrentAuthor = manager.GetSelectedAuthor();

            IsActive = CurrentAuthor.IsActive;

            NameBox.Text = CurrentAuthor.Name;
            VCSEmailBox.Text = CurrentAuthor.VCSEmail;
            LocalObservables.Brush = new SolidColorBrush(CurrentAuthor.Color);

            foreach (var author in manager.GetAllAuthors())
            {
                if (!CurrentAuthor.ContainsEmail(author.Email))
                {
                    OtherAuthors.Add(author);
                }
            }

            foreach (var subAuthor in CurrentAuthor.SubAuthors)
            {
                SubAuthors.Add(subAuthor);
            }

            UpdateAliasesAndEmails();


            foreach (var team in manager.GetAllTeams())
            {
                Teams.Add(team);
            }

            if (CurrentAuthor.Teams.Count != 0)
            {
                LocalObservables.SelectedTeams = CurrentAuthor.Teams.Select(x => x).ToObservableCollection();
            }
            else
            {
                LocalObservables.SelectedTeams = new ObservableCollection<PCOTeam>();
            }
            foreach(var team in CurrentAuthor.Teams)
            {
                TeamsDataGrid.SelectedItems.Add(team);
            }
            UpdateTeamsString();

            ActiveSlider.SelectedIndex = IsActive ? 0 : 1;

        }

        private void UpdateAliasesAndEmails()
        {
            Emails.Clear();
            Aliases.Clear();

            Emails.Add(CurrentAuthor.Email);
            foreach (var alias in CurrentAuthor.Aliases)
            {
                Aliases.Add(alias);
            }
            foreach (var subAuthor in SubAuthors)
            {
                Emails.Add(subAuthor.Email);
                foreach (var alias in subAuthor.Aliases)
                {
                    Aliases.Add(alias);
                }
            }

        }

        private void SfColorPicker_SelectedBrushChanged(object sender, Syncfusion.UI.Xaml.Editors.SelectedBrushChangedEventArgs e)
        {
            LocalObservables.Brush = (SolidColorBrush)e.NewBrush;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            var manager = PCOState.GetInstance().GetContributorState();
            manager.SetAuthorUpdated(false);
            manager.SetSelectedAuthor(null);
            manager.GetCurrentAuthorDialog().Hide();
            manager.SetCurrentAuthorDialog(null);
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var manager = PCOState.GetInstance().GetContributorState();

            if (NameBox.Text.Length == 0)
            {
                LocalObservables.NameFlyoutMsg = "Your Author must have a name.";
                ShowNameFlyout();
                return;
            }
            if (VCSEmailBox.Text.Length > 0 && !VCSEmailBox.Text.ToLower().Equals(CurrentAuthor.VCSEmail) && !manager.CheckTeamVCSEmailAvailable(VCSEmailBox.Text))
            {
                LocalObservables.EmailFlyoutMsg = "The author can not have the same VCS Email as another author.";
                ShowEmailFlyout();
                return;
            }

            CurrentAuthor.Name = NameBox.Text;
            CurrentAuthor.VCSEmail = VCSEmailBox.Text.ToLower();

            CurrentAuthor.Color = LocalObservables.Brush.Color;

            CurrentAuthor.IsActive = IsActive;

            CurrentAuthor.EmptySubAuthors();
            foreach (var subAuthor in SubAuthors)
            {
                CurrentAuthor.ConnectAuthor(subAuthor);
            }

            if (CurrentAuthor.Teams.Count != 0)
            {
                var previousTeams = CurrentAuthor.Teams.ToList();
                foreach(var team in previousTeams)
                {
                    CurrentAuthor.DisconnectFromTeam(team);
                }
            }

            if (LocalObservables.SelectedTeams.Count != 0)
            {
                foreach(var team in LocalObservables.SelectedTeams)
                {
                    CurrentAuthor.ConnectToTeam(team);
                }
            }

            manager.SetAuthorUpdated(true);
            manager.SetSelectedAuthor(null);
            manager.GetCurrentAuthorDialog().Hide();
            manager.SetCurrentAuthorDialog(null);

        }

        public void DeleteClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var author = button.DataContext as Author;

            SubAuthors.Remove(author);
            OtherAuthors.Add(author);
            UpdateAliasesAndEmails();
        }

        public void AddClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var author = button.DataContext as Author;

            OtherAuthors.Remove(author);
            SubAuthors.Add(author);
            UpdateAliasesAndEmails();
        }

        private void ShowNameFlyout()
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)NameBox);
        }
        private void ShowEmailFlyout()
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)VCSEmailBox);
        }

        private void SfComboBox_SelectionChanged(object sender, Syncfusion.UI.Xaml.Editors.ComboBoxSelectionChangedEventArgs e)
        {
            //var co = 2;
        }

        private void ActivityChanged(object sender, Syncfusion.UI.Xaml.Editors.SegmentSelectionChangedEventArgs e)
        {
            var resourceDict = ActiveSlider.Resources.ThemeDictionaries.First().Value as ResourceDictionary;
            var selectedBrush = resourceDict.Values.First() as SolidColorBrush;
            if (!IsInitialized)
            {
                selectedBrush.Color = IsActive ? Color.FromArgb(255, 82, 139, 82) : Color.FromArgb(255, 205, 92, 92);
                IsInitialized = true;
                return;
            }
            if (e.NewValue.Equals("Active"))
            {
                IsActive = true;
            }
            else if (e.NewValue.Equals("Inactive"))
            {
                IsActive = false;
            }
            selectedBrush.Color = IsActive ? Color.FromArgb(255, 82, 139, 82) : Color.FromArgb(255, 205, 92, 92);
        }

        private void TeamSelectionChanged(object sender, Syncfusion.UI.Xaml.Grids.GridSelectionChangedEventArgs e)
        {
           
            foreach(var addedItem in e.AddedItems)
            {
                var team = ((GridRowInfo)addedItem).RowData as PCOTeam;
                if (! LocalObservables.SelectedTeams.Contains(team)) { 
                    LocalObservables.SelectedTeams.Add(team);
                }
            }
            foreach(var removedItem in e.RemovedItems)
            {
                var team = ((GridRowInfo)removedItem).RowData as PCOTeam;
                if (LocalObservables.SelectedTeams.Contains(team))
                {
                    LocalObservables.SelectedTeams.Remove(team);
                }
            }
            UpdateTeamsString();
        }
        private void UpdateTeamsString()
        {
            string teamsString = "";
            foreach(var team in LocalObservables.SelectedTeams)
            {
                teamsString += team.Name + ", ";
            }
            if (teamsString.Length > 0)
            {

                teamsString = teamsString.Substring(0, teamsString.Length - 2);
            }
            LocalObservables.SelectedTeamsString = teamsString;
        }
    }
}


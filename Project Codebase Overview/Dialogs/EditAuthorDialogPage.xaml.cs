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
        public PCOTeam NoTeamObject;
        private class Observables : ObservableObject
        {
            private SolidColorBrush _brush;
            public SolidColorBrush Brush { get => _brush; set => SetProperty(ref _brush, value); }
            private string _nameFlyoutMsg;
            public string NameFlyoutMsg { get => _nameFlyoutMsg; set => SetProperty(ref _nameFlyoutMsg, value); }
            private string _teamNameMsg;
            public string TeamNameMsg { get => _teamNameMsg; set => SetProperty(ref _teamNameMsg, value); }
            private PCOTeam _selectedTeam;
            public PCOTeam SelectedTeam { get => _selectedTeam; set => SetProperty(ref _selectedTeam, value); }
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

            NoTeamObject = new PCOTeam("No Team", PCOColorPicker.Black);
            
            var manager = PCOState.GetInstance().GetContributorState();
            CurrentAuthor = manager.GetSelectedAuthor();

            NameBox.Text = CurrentAuthor.Name;
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


            Teams.Add(NoTeamObject);
            foreach (var team in manager.GetAllTeams())
            {
                Teams.Add(team);
            }

            if (CurrentAuthor.Team != null)
            {
                LocalObservables.SelectedTeam = CurrentAuthor.Team;
            }
            else
            {
                LocalObservables.SelectedTeam = NoTeamObject;
            }

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

            CurrentAuthor.Name = NameBox.Text;
            CurrentAuthor.Color = LocalObservables.Brush.Color;

            CurrentAuthor.EmptySubAuthors();
            foreach (var subAuthor in SubAuthors)
            {
                CurrentAuthor.ConnectAuthor(subAuthor);
            }

            if (CurrentAuthor.Team != null)
            {
                CurrentAuthor.DisconnectFromTeam();
            }

            if (LocalObservables.SelectedTeam != NoTeamObject)
            {
                CurrentAuthor.ConnectToTeam(LocalObservables.SelectedTeam);
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

        private void SfComboBox_SelectionChanged(object sender, Syncfusion.UI.Xaml.Editors.ComboBoxSelectionChangedEventArgs e)
        {
            //var co = 2;
        }
    }
}

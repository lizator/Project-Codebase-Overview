using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Color = Windows.UI.Color;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddEditTeamDialogPage : Page
    {

        private PCOTeam Team;
        private Dictionary<string, bool> IsAuthorInTeam;
        private Dictionary<string, Author> AuthorList;
        private bool IsVCSIDSet;

        private static string NO_TEAM_HEADER_TEXT = "No team";
        private class Observables: ObservableObject
        {
            private SolidColorBrush _brush;
            public SolidColorBrush Brush { get => _brush; set => SetProperty(ref _brush, value); }

            private string _nameFlyoutMsg;
            public string NameFlyoutMsg { get => _nameFlyoutMsg; set => SetProperty(ref _nameFlyoutMsg, value); }
            private Visibility _deleteVisibility = Visibility.Collapsed;
            public Visibility DeleteVisibility { get => _deleteVisibility; set => SetProperty(ref _deleteVisibility, value); }
            private string _confirmDeleteMsg;
            public string ConfirmDeleteMsg { get => _confirmDeleteMsg; set => SetProperty(ref _confirmDeleteMsg, value); }
        }
        private Observables LocalObservables;
        

        private bool IsTeamNew = false;

        private ObservableCollection<ViewAuthor> _unselectedAuthorList;
        public ObservableCollection<ViewAuthor> UnselectedAuthorList
        {
            get => _unselectedAuthorList;
            set => _unselectedAuthorList = value;
        }

        private ObservableCollection<Author> _selectedAuthorList;
        public ObservableCollection<Author> SelectedAuthorList
        {
            get => _selectedAuthorList;
            set => _selectedAuthorList = value;
        }

        public AddEditTeamDialogPage()
        {
            this.InitializeComponent();
            LocalObservables = new Observables();
            IsAuthorInTeam = new Dictionary<string, bool>();
            AuthorList = new Dictionary<string, Author>();
            Team = PCOState.GetInstance().GetContributorState().GetSelectedTeam();
            if (Team == null)
            {
                IsTeamNew = true;
                Team = new PCOTeam();
                Team.Color = PCOColorPicker.GetInstance().AssignTeamColor();
                IsVCSIDSet = false;
            } else
            {
                IsVCSIDSet = Team.VCSID.Length > 0;
                LocalObservables.DeleteVisibility = Visibility.Visible;
                LocalObservables.ConfirmDeleteMsg = "Confirm deleting team \"" + Team.Name + "\"";
            }

            LocalObservables.Brush = new SolidColorBrush(Team.Color);

            foreach (var author in PCOState.GetInstance().GetContributorState().GetAllAuthors())
            {
                IsAuthorInTeam.Add(author.Email, Team.ContainsEmail(author.Email));
                AuthorList.Add(author.Email, author);
            }

            NameBox.Text = Team.Name;
            VCSIDBox.Text = Team.VCSID;

            UnselectedAuthorList = new ObservableCollection<ViewAuthor>();
            SelectedAuthorList = new ObservableCollection<Author>();
            UnselectedAuthors.Source = new ObservableCollection<GroupInfoList>();

            addLateLoad();
        }

        private async Task<object> addLateLoad()
        {

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            await System.Threading.Tasks.Task.Run(() =>
            {
                Thread.Sleep(50);
                dispatcherQueue.TryEnqueue(() =>
                    UpdateAuthorLists());
            });
            return null;
        }

        private void AddClicked(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).DataContext as ViewAuthor;
            IsAuthorInTeam[item.Email] = true;

            UpdateAuthorLists(SearchBox.Text);
        }

        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).DataContext as Author;
            IsAuthorInTeam[item.Email] = false;
            UpdateAuthorLists(SearchBox.Text);
            
        }

        private void DeleteTeam(object sender, RoutedEventArgs e)
        {
            var manager = PCOState.GetInstance().GetContributorState();

            manager.DeleteTeam(Team);

            manager.SetTeamUpdated(true);
            manager.SetSelectedTeam(null);
            manager.GetCurrentTeamDialog().Hide();
            manager.SetCurrentTeamDialog(null);
        }

        

        private bool NotSelectedFilter(KeyValuePair<string, Author> pair, string searchString)
        {
            if (!IsAuthorInTeam.GetValueOrDefault(pair.Key))
            {
                var isSearching = searchString.Length > 0;
                if (!isSearching)
                {
                    return true;
                }

                var nameFound = pair.Value.Name.ToLower().Contains(searchString.ToLower());
                var emailFound = pair.Value.Email.ToLower().Contains(searchString.ToLower());
                bool teamFound;
                if (pair.Value.Teams.Count == 0)
                {
                    teamFound = NO_TEAM_HEADER_TEXT.ToLower().Contains(searchString.ToLower());
                } else
                {
                    teamFound = pair.Value.Teams.Where(x => x.Name.ToLower().Contains(searchString.ToLower())).Any();
                }
                return nameFound || emailFound || teamFound;
            }
            return false;
        }
        private void UpdateAuthorLists(string searchString = "")
        {
            var isSearching = searchString.Length > 0;

            ((ObservableCollection<GroupInfoList>)UnselectedAuthors.Source).Clear();

            UnselectedAuthorList.Clear();



            AuthorList.Where(pair => NotSelectedFilter(pair, searchString))
            .Select(pair => pair.Value).ForEach(author => { 
                if (author.Teams.Count == 0)
                {
                    UnselectedAuthorList.Add(new ViewAuthor() { Name = author.Name, Email = author.Email, TeamName = NO_TEAM_HEADER_TEXT });
                } else
                {
                    author.Teams.ForEach(team => {
                        if (team.Name.Equals(Team.Name))
                        {
                            if (author.Teams.Count == 1)
                            {
                                UnselectedAuthorList.Add(new ViewAuthor() { Name = author.Name, Email = author.Email, TeamName = NO_TEAM_HEADER_TEXT });
                            }
                        }
                        else
                        {
                            UnselectedAuthorList.Add(new ViewAuthor() { Name = author.Name, Email = author.Email, TeamName = team.Name });
                        }
                    });
                }
            });

            //simple grouping
            var groups = UnselectedAuthorList.GroupBy(item => item.TeamName);
            //add extras 
            

            var query = groups
                .OrderBy(item => (item.Key.Equals(NO_TEAM_HEADER_TEXT)) ? 0 : 1)
                .ThenBy(item => item.Key)
                .Select(item => new GroupInfoList(item) { Key = item.Key });

            query.ForEach(item => ((ObservableCollection<GroupInfoList>)UnselectedAuthors.Source).Add(item));

            SelectedAuthorList.Clear();
            AuthorList.Where(pair => IsAuthorInTeam.GetValueOrDefault(pair.Key))
                .Select(pair => pair.Value).OrderBy(author => author.Name).ToList().ForEach(author => SelectedAuthorList.Add(author));

        }

       

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            var manager = PCOState.GetInstance().GetContributorState();
            manager.SetTeamUpdated(false);
            manager.SetSelectedTeam(null);
            manager.GetCurrentTeamDialog().Hide();
            manager.SetCurrentTeamDialog(null);
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var manager = PCOState.GetInstance().GetContributorState();

            if (NameBox.Text.Length == 0)
            {
                LocalObservables.NameFlyoutMsg = "Your team must have a name.";
                ShowNameFlyout();
                return;
            }

            if (!NameBox.Text.Equals(Team.Name) && !manager.CheckTeamNameAvailable(NameBox.Text))
            {
                LocalObservables.NameFlyoutMsg = "Your team can't be named the same as another team.";
                ShowNameFlyout();
                return;
            }

            var newVCSID = VCSIDBox.Text.StartsWith('@') || VCSIDBox.Text.Length == 0 ? VCSIDBox.Text : "@" + VCSIDBox.Text;
            if (newVCSID.Length > 0 && !newVCSID.Equals(Team.VCSID) && !manager.CheckTeamVCSIDAvailable(newVCSID))
            {
                ShowVCSIDFlyout();
                return;
            }
            if (Team.Name != null && Team.Name != "" && !Team.Name.Equals(NameBox.Text))
            {
                manager.RenameTeam(Team.Name, NameBox.Text);
            }
            Team.Name = NameBox.Text;
            Team.VCSID = newVCSID;
            Team.EmptyMembers();

            IsAuthorInTeam.Where(x => x.Value).ForEach(x => Team.ConnectMember(AuthorList[x.Key]));

            Team.Color = LocalObservables.Brush.Color;


            if (IsTeamNew)
            {
                manager.AddTeam(Team);
                manager.SetTeamUpdated(true);
            }


            manager.SetSelectedTeam(null);
            manager.GetCurrentTeamDialog().Hide();
            manager.SetCurrentTeamDialog(null);

        }

        private void ShowNameFlyout()
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)NameBox);
        }

        private void ShowVCSIDFlyout()
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)VCSIDBox);
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAuthorLists(SearchBox.Text);
        }
        private void SfColorPicker_SelectedBrushChanged(object sender, Syncfusion.UI.Xaml.Editors.SelectedBrushChangedEventArgs e)
        {
            LocalObservables.Brush = (SolidColorBrush)e.NewBrush;
        }

        private void DeleteConfirm_Checked(object sender, RoutedEventArgs e)
        {
            ConfirmDeleteBtn.IsEnabled = true;
        }

        private void DeleteConfirm_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfirmDeleteBtn.IsEnabled = false;
        }

        private void VCSIDBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vcsidbox = ((TextBox)sender);
            var state = vcsidbox.FocusState;
            if (state != FocusState.Unfocused)
            {
                IsVCSIDSet = true;
            }
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsVCSIDSet)
            {
                VCSIDBox.Text = NameBox.Text.Length > 0 ? "@" + NameBox.Text.ToLower().Replace(' ', '_') : "";
            }
        }
    }
    public class GroupInfoList : ObservableCollection<object>
    {
        public GroupInfoList(IEnumerable<object> items) : base(items)
        {
        }
        public object Key { get; set; }
    }
    public class ViewAuthor
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string TeamName { get; set; }
    }
}

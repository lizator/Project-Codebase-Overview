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

        private static string NO_TEAM_HEADER_TEXT = "No team";
        private class Observables: ObservableObject
        {
            private SolidColorBrush _brush;
            public SolidColorBrush Brush { get => _brush; set => SetProperty(ref _brush, value); }

            private string _nameFlyoutMsg;
            public string NameFlyoutMsg { get => _nameFlyoutMsg; set => SetProperty(ref _nameFlyoutMsg, value); }
        }
        private Observables LocalObservables;
        

        private bool IsTeamNew = false;

        private ObservableCollection<Author> _unselectedAuthorList;
        public ObservableCollection<Author> UnselectedAuthorList
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
            Team = ContributorManager.GetInstance().GetSelectedTeam();
            if (Team == null)
            {
                IsTeamNew = true;
                Team = new PCOTeam();
                Team.Color = PCOColorPicker.GetInstance().AssignTeamColor();
            }

            LocalObservables.Brush = new SolidColorBrush(Team.Color);

            foreach (var author in ContributorManager.GetInstance().GetAllAuthors())
            {
                IsAuthorInTeam.Add(author.Email, Team.ContainsEmail(author.Email));
                AuthorList.Add(author.Email, author);
            }

            NameBox.Text = Team.Name;

            UnselectedAuthorList = new ObservableCollection<Author>();
            SelectedAuthorList = new ObservableCollection<Author>();
            UnselectedAuthors.Source = new ObservableCollection<GroupInfoList>();

            addLateLoad();
        }

        private async Task<object> addLateLoad()
        {

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            await System.Threading.Tasks.Task.Run(() =>
            {
                Thread.Sleep(20);
                dispatcherQueue.TryEnqueue(() =>
                    UpdateAuthorLists());
            });
            return null;
        }

        private void AddClicked(object sender, RoutedEventArgs e)
        {
            if (UnselectedListView.SelectedItems.Count() > 0)
            {
                foreach(var item in UnselectedListView.SelectedItems)
                {
                    var author = item as Author;
                    IsAuthorInTeam[author.Email] = true;
                }
            }
            UpdateAuthorLists(SearchBox.Text);
        }

        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedListView.SelectedItems.Count() > 0)
            {
                foreach(var item in SelectedListView.SelectedItems)
                {
                    var author = item as Author;
                    IsAuthorInTeam[author.Email] = false;
                }
                UpdateAuthorLists(SearchBox.Text);
            }
        }

        private void UpdateAuthorLists(string searchString = "")
        {
            var isSearching = searchString.Length > 0;

            ((ObservableCollection<GroupInfoList>)UnselectedAuthors.Source).Clear();

            UnselectedAuthorList.Clear();
            AuthorList.Where(pair =>
                !IsAuthorInTeam.GetValueOrDefault(pair.Key) &&
                (!isSearching || (
                    pair.Value.Name.ToLower().Contains(searchString.ToLower()) ||
                    pair.Value.Email.ToLower().Contains(searchString.ToLower()) ||
                    (
                        pair.Value.Team == null ?
                            NO_TEAM_HEADER_TEXT.ToLower().Contains(searchString.ToLower()) :
                            pair.Value.Team.Name.ToLower().Contains(searchString.ToLower())
                    )
                ))
            )
            .Select(pair => pair.Value).OrderBy(author => author.Name).ToList().ForEach(author => UnselectedAuthorList.Add(author));

            var query = UnselectedAuthorList.GroupBy(item => (item.Team == null ? NO_TEAM_HEADER_TEXT : item.Team.Name))
                .OrderBy(item => (item.Key.Equals(NO_TEAM_HEADER_TEXT)) ? 0 : 1)
                .ThenBy(item => item.Key)
                .Select(item => new GroupInfoList(item) { Key = item.Key });

            query.ForEach(item => ((ObservableCollection<GroupInfoList>)UnselectedAuthors.Source).Add(item));

            SelectedAuthorList.Clear();
            AuthorList.Where(pair =>
                IsAuthorInTeam.GetValueOrDefault(pair.Key) &&
                (!isSearching || (pair.Value.Name.ToLower().Contains(searchString.ToLower()) || pair.Value.Email.ToLower().Contains(searchString.ToLower())))
            )
            .Select(pair => pair.Value).OrderBy(author => author.Name).ToList().ForEach(author => SelectedAuthorList.Add(author));

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAuthorLists(SearchBox.Text);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            var manager = ContributorManager.GetInstance();
            manager.SetTeamUpdated(false);
            manager.SetSelectedTeam(null);
            manager.GetCurrentTeamDialog().Hide();
            manager.SetCurrentTeamDialog(null);
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var manager = ContributorManager.GetInstance();

            if (NameBox.Text.Length == 0)
            {
                LocalObservables.NameFlyoutMsg = "Your team need a name.";
                ShowNameFlyout();
                return;
            }

            if (!NameBox.Text.Equals(Team.Name) && !manager.CheckTeamNameAvailable(NameBox.Text))
            {
                LocalObservables.NameFlyoutMsg = "Your team can't be named the same as another team.";
                ShowNameFlyout();
                return;
            }

            Team.Name = NameBox.Text;
            Team.EmptyMembers();
            foreach (var pair in IsAuthorInTeam)
            {
                if (pair.Value)
                {
                    Team.ConnectMember(AuthorList[pair.Key]);
                }
            }
            Team.Color = LocalObservables.Brush.Color;


            if (IsTeamNew)
            {
                manager.AddTeam(Team);
            }


            manager.SetTeamUpdated(true);
            manager.SetSelectedTeam(null);
            manager.GetCurrentTeamDialog().Hide();
            manager.SetCurrentTeamDialog(null);

        }

        private void ShowNameFlyout()
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)NameBox);
        }

        private void SfColorPicker_SelectedBrushChanged(object sender, Syncfusion.UI.Xaml.Editors.SelectedBrushChangedEventArgs e)
        {
            LocalObservables.Brush = (SolidColorBrush)e.NewBrush;
        }
    }
    public class GroupInfoList : List<object>
    {
        public GroupInfoList(IEnumerable<object> items) : base(items)
        {
        }
        public object Key { get; set; }
    }
}

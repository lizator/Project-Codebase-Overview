using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
            IsAuthorInTeam = new Dictionary<string, bool>();
            AuthorList = new Dictionary<string, Author>();
            Team = ContributorManager.GetInstance().GetSelectedTeam();
            if (Team == null)
            {
                IsTeamNew = true;
                Team = new PCOTeam();
                Team.Color = PCOColorPicker.GetInstance().AssignAuthorColor();
            }

            foreach (var author in ContributorManager.GetInstance().GetAllAuthors())
            {
                IsAuthorInTeam.Add(author.Email, Team.ContainsEmail(author.Email));
                AuthorList.Add(author.Email, author);
            }

            NameBox.Text = Team.Name;

            UnselectedAuthorList = new ObservableCollection<Author>();
            SelectedAuthorList = new ObservableCollection<Author>();

            UpdateAuthorLists();
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
                UpdateAuthorLists(SearchBox.Text);
            }
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
            UnselectedAuthorList.Clear();
            AuthorList.Where(pair =>
                !IsAuthorInTeam.GetValueOrDefault(pair.Key) &&
                (!isSearching || (pair.Value.Name.ToLower().Contains(searchString.ToLower()) || pair.Value.Email.ToLower().Contains(searchString.ToLower())))
            )
            .Select(pair => pair.Value).OrderBy(author => author.Name).ToList().ForEach(author => UnselectedAuthorList.Add(author));


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
            manager.SetCurrentTeamDialog(null);
            manager.GetCurrentTeamDialog().Hide();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            Team.Name = NameBox.Text;
            Team.EmptyMembers();
            foreach(var pair in IsAuthorInTeam)
            {
                if (pair.Value)
                {
                    Team.ConnectMember(AuthorList[pair.Key]);
                }
            }

            var manager = ContributorManager.GetInstance();

            if (IsTeamNew)
            {
                manager.AddTeam(Team);
            }

            manager.SetTeamUpdated(true);
            manager.SetSelectedTeam(null);
            manager.GetCurrentTeamDialog().Hide();
            manager.SetCurrentTeamDialog(null);

        }
    }
}

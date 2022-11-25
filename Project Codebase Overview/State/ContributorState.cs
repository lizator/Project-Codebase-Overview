using Microsoft.UI.Xaml.Controls;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.Settings;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.ContributorManagement
{
    public class ContributorState
    {
        Dictionary<string, Author> Authors;
        Dictionary<string, Author> FileCreators; //Dict <filepath, creator> 
        Dictionary<string, PCOTeam> Teams; //Dict <Team.Name, Team> 
        PCOTeam NoTeam;
        PCOTeam SelectedTeam;
        ContentDialog CurrentTeamDialog;
        Author SelectedAuthor;
        ContentDialog CurrentAuthorDialog;
        bool TeamUpdated = false;
        bool AuthorUpdated = false;

        public ContributorState()
        {
            Authors = new Dictionary<string, Author>();
            FileCreators = new Dictionary<string, Author>();
            Teams = new Dictionary<string, PCOTeam>();
            NoTeam = new PCOTeam("No Team", PCOColorPicker.Black);
        }

        public PCOTeam GetNoTeam() { return NoTeam; }

        public void UpdateCreator(string path, string email)
        {
            FileCreators[path] = GetAuthor(email);
        }

        public Author GetCreatorFromPath(string path)
        {
            if (FileCreators.TryGetValue(path, out Author creator))
            {
                if (creator.OverAuthor != null)
                {
                    return creator.OverAuthor;
                }
                return creator;
            }
            return null;
        }

        private void AddAuthor(string email, string name, Color? color)
        {
            Author author = new Author(email, name);
            var colorPicker = PCOColorPicker.GetInstance();
            author.Color = color ?? colorPicker.AssignAuthorColor();
            this.Authors.Add(email, author);
        }

        public Author GetAuthor (string email)
        {
            if (this.Authors.TryGetValue(email, out Author author))
            {
                if (author.OverAuthor != null)
                {
                    return author.OverAuthor;
                }
                return author;
            }
            Debug.WriteLine("Author with email " + email + " has been created late");
            InitializeAuthor(email, "no name", null);
            return this.Authors[email];
        }

        public void InitializeAuthor(string email, string name, Color? color)
        {
            if (this.Authors.ContainsKey(email))
            {
                this.Authors[email].AddAlias(name);
            } else
            {
                this.AddAuthor(email, name, color);
            }
        }

        public List<Author> GetAllAuthors()
        {
            return Authors.Values.Where(author => author.OverAuthor == null).OrderBy(author => author.Name).ToList();
        }

        public List<PCOTeam> GetAllTeams()
        {
            return Teams.Values.ToList();
        }
        public List<IOwner> GetAllOwners()
        {
            if(PCOState.GetInstance().GetSettingsState().CurrentMode == PCOExplorerMode.USER)
            {
                return GetAllAuthors().Where(x => x.IsActive).Select(x => (IOwner)x).ToList();
            }
            else
            {
                //Mode.TEAMS
                return GetAllTeams().Where(x => x.IsActive).Select(x => (IOwner)x).ToList();
            }
        }

        public void SetSelectedTeam(PCOTeam team)
        {
            SelectedTeam = team;
        }
        
        public PCOTeam GetSelectedTeam()
        {
            return SelectedTeam;
        }

        public void SetCurrentTeamDialog(ContentDialog dialog)
        {
            CurrentTeamDialog = dialog;
        }

        public ContentDialog GetCurrentTeamDialog()
        {
            return CurrentTeamDialog;
        }

        public void SetTeamUpdated(bool update)
        {
            TeamUpdated = update;
        }

        public bool GetTeamUpdated()
        {
            return TeamUpdated;
        }

        public bool CheckTeamNameAvailable(string name)
        {
            if (Teams.Keys.Where(key => key.ToLower().Equals(name.ToLower())).Any())
            {
                return false;
            }
            return true;
        }

        public void AddTeam(PCOTeam team)
        {
            if (!CheckTeamNameAvailable(team.Name))
            {
                throw new Exception("Team with the same name already exists");
            }
            Teams.Add(team.Name, team);
        }

        public void DeleteTeam(PCOTeam team)
        {
            if (Teams.TryGetValue(team.Name, out var pCOTeam)) {
                team.EmptyMembers();
                Teams.Remove(team.Name);
            }
        }

        public void SetSelectedAuthor(Author author)
        {
            SelectedAuthor = author;
        }

        public Author GetSelectedAuthor()
        {
            return SelectedAuthor;
        }

        public void SetAuthorUpdated(bool update)
        {
            AuthorUpdated = update;
        }

        public bool GetAuthorUpdated()
        {
            return AuthorUpdated;
        }

        public void SetCurrentAuthorDialog(ContentDialog dialog)
        {
            CurrentAuthorDialog = dialog;
        }

        public ContentDialog GetCurrentAuthorDialog()
        {
            return CurrentAuthorDialog;
        }

        public void RenameTeam(string origName, string newName)
        {
            var tmpTeam = Teams[origName];
            Teams.Remove(origName);
            Teams.Add(newName, tmpTeam);
        }
    }
}

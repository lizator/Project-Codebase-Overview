using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    public class Author: ObservableObject, IOwner
    {
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private Color _color;
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        private string _vCSEmail;
        public string VCSEmail { get => _vCSEmail; set => SetProperty(ref _vCSEmail, value); }
        public ObservableCollection<string> Aliases { get; }

        private Author _overAuthor;
        public Author OverAuthor { get => _overAuthor; set => SetProperty(ref _overAuthor, value); }
        public ObservableCollection<Author> SubAuthors { get; set; }
        private ObservableCollection<PCOTeam> _teams;
        public ObservableCollection<PCOTeam> Teams { get => _teams; set => SetProperty(ref _teams, value); }
        private int _subAuthorCount;
        public int SubAuthorCount { get => _subAuthorCount; set => SetProperty(ref _subAuthorCount, value); }
        private bool _isActive;
        public bool IsActive { get => _isActive; set => SetIsActive(value); }
        private void SetIsActive(bool value)
        {
            SetProperty(ref _isActive, value);
            ActiveSymbol = _isActive ? Symbol.Accept : Symbol.Cancel;
            foreach(var team in Teams)
            {
                team?.UpdateIsActive();
            }
        }

        private Symbol _activeSymbol;
        public Symbol ActiveSymbol { get => _activeSymbol; set => SetProperty(ref _activeSymbol, value); }

        public Author(string email, string name)
        {
            Teams = new ObservableCollection<PCOTeam>();
            this.Name = name;
            this.Email = email;
            this.VCSEmail = email;
            this.Aliases = new ObservableCollection<string> { name };
            SubAuthors = new ObservableCollection<Author>();
            SubAuthorCount = 0;
            IsActive = true;
        }



        public bool ContainsEmail(string email)
        {
            return Email.Equals(email) || SubAuthors.Where(sub => sub.Email.Equals(email)).Any();
        }

        public void AddAlias(string alias)
        {
            if (!Aliases.Contains(alias))
            {
                this.Aliases.Add(alias);
            }
        }

        public void ConnectAuthor(Author otherAuthor)
        {
            if(otherAuthor.Teams.Count != 0)
            {
                var otherAuthorTeams = otherAuthor.Teams.ToList();
                foreach(var team in otherAuthorTeams)
                {
                    otherAuthor.DisconnectFromTeam(team);
                }
            }
            if (otherAuthor.OverAuthor != null)
            {
                otherAuthor.DisconnectFromOverAuthor();
            }

            otherAuthor.OverAuthor = this;
            this.SubAuthors.Add(otherAuthor);
            SubAuthorCount++;

            while (otherAuthor.SubAuthors.Count > 0)
            {
                var subAuthor = otherAuthor.SubAuthors.First();
                this.ConnectAuthor(subAuthor);
            }
        }

        public void DisconnectFromOverAuthor()
        {
            if (OverAuthor == null)
            {
                throw new Exception("Cannot disconnect Author. No overauthor is set");
            }
            this.OverAuthor.SubAuthorCount--;
            this.OverAuthor.SubAuthors.Remove(this);
            this.OverAuthor = null;
        }

        public void EmptySubAuthors()
        {
            while (SubAuthors.Count > 0)
            {
                var subAuthor = SubAuthors.First();
                subAuthor.DisconnectFromOverAuthor();
            }
            SubAuthorCount = 0;
        }

        public void ConnectToTeam(PCOTeam team)
        {
            team.ConnectMember(this);
        }
        public void DisconnectFromTeam(PCOTeam team)
        {
            team.RemoveMember(this);
            this.Teams.Remove(team);
        }
    }
}

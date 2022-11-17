using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    public class PCOTeam : ObservableObject, IOwner 
    {
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private Color _color;
        public Color Color { get => _color; set => SetProperty(ref _color, value); }

        private string _membersString;
        public string MemberString { get => _membersString; set => SetProperty(ref _membersString, value); }
        private string _moreString;
        public string MoreString { get => _moreString; set => SetProperty(ref _moreString, value); }
        private Visibility _moreVisibility = Visibility.Collapsed;
        public Visibility MoreVisibility { get => _moreVisibility; set => SetProperty(ref _moreVisibility, value); }
        
        public ObservableCollection<Author> Members { get; set; }

        public PCOTeam(string name, Color color)
        {
            Name = name;
            Color = color;
            Members = new ObservableCollection<Author>();
            UpdateMemberString();
        }

        public PCOTeam()
        {
            Members = new ObservableCollection<Author>();
            UpdateMemberString();
        }

        public bool ContainsEmail(string email)
        {
            return Members.Where(x => x.ContainsEmail(email)).Any();
        }

        public void ConnectMember(Author member)
        {
            if (member.OverAuthor != null)
            {
                throw new Exception("Cannot add subauthor to a Team");
            }

            if (member.Team != null)
            {
                member.Team.RemoveMember(member);
            }
            member.Team = this;
            this.Members.Add(member);

            UpdateMemberString();

        }

        public void EmptyMembers()
        {
            while (Members.Count > 0)
            {
                var member = Members.First();
                member.DisconnectFromTeam();
            }
        }

        public void RemoveMember(Author member)
        {
            if (this.Members.Contains(member))
            {
                member.Team = null;
                this.Members.Remove(member);
                UpdateMemberString();
            }
        }

        private void UpdateMemberString()
        {
            int shownItemCount;
            int maxShownItemsWOMore = 8;

            if (Members.Count() > maxShownItemsWOMore)
            {
                shownItemCount = maxShownItemsWOMore - 1;
                MoreString = "+" + (Members.Count() - shownItemCount) + " more";
                MoreVisibility = Visibility.Visible;
            }
            else
            {
                shownItemCount = Members.Count();
                MoreString = "";
                MoreVisibility = Visibility.Collapsed;
            }
            StringBuilder stringBuilder = new StringBuilder();
            var visibleMembers = Members.ToList().GetRange(0, shownItemCount);
            foreach (var member in visibleMembers)
            {
                stringBuilder.AppendLine(member.Name);
            }

            if (shownItemCount == 0)
            {
                stringBuilder.AppendLine("No members.");
                shownItemCount = 1;
            }

            int whitespaceCount = maxShownItemsWOMore - shownItemCount;
            for (int i = 0; i < whitespaceCount; i++)
            {
                stringBuilder.AppendLine(" ");
            }
            MemberString = stringBuilder.ToString().Substring(0, stringBuilder.Length - 2);
            
        }

    }
}

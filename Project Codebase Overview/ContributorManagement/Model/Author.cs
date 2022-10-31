using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    public class Author:IOwner
    {
        public string Name { get; set; }
        List<string> Aliases { get; }
        public string Email { get; set; }
        public Color Color { get; set; }

        public Author OverAuthor { get; set; }
        public List<Author> SubAuthors { get; set; }
        public PCOTeam Team { get; set; }

        public Author(string email, string name)
        {
            this.Name = name;
            this.Email = email;
            this.Aliases = new List<string> { name };
            SubAuthors = new List<Author>();
        }

        public bool ContainsEmail(string email)
        {
            return Email.Equals(email) || SubAuthors.Where(sub => sub.Equals(email)).Any();
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
            if(otherAuthor.Team != null)
            {
                otherAuthor.Team.RemoveMember(otherAuthor);
            }

            otherAuthor.OverAuthor = this;
            this.SubAuthors.Add(otherAuthor);

            foreach (var subAuthor in otherAuthor.SubAuthors)
            {
                this.ConnectAuthor(subAuthor);
                otherAuthor.SubAuthors.Remove(subAuthor);
            }
        }

        public void DisconnectFromOverAuthor()
        {
            if (OverAuthor == null)
            {
                throw new Exception("Cannot disconnect author. No overauthor is set");
            }
            this.OverAuthor.SubAuthors.Remove(this);
            this.OverAuthor = null;
        }
    }
}

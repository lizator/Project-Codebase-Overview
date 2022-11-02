using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    public class PCOTeam : IOwner
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public List<Author> Members { get; set; }

        public PCOTeam()
        {
            Members = new List<Author>();
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

        }

        public void RemoveMember(Author member)
        {
            this.Members.Remove(member);
            member.Team = null;
        }

    }
}

using System;
using System.Collections.Generic;
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
        string Email { get; set; }
        public Color Color { get; set; }

        public Author(string email, string name)
        {
            this.Name = name;
            this.Email = email;
            this.Aliases = new List<string> { name };
        }

        public void AddAlias(string alias)
        {
            if (!Aliases.Contains(alias))
            {
                this.Aliases.Add(alias);
            }
        }
    }
}

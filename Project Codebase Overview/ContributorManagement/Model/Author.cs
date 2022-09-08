using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    internal class Author
    {
        int id { get; }
        string name { get; set; }
        string email { get; set; }

        public Author(int id, string email, string name)
        {
            this.id = id;
            this.name = name;
            this.email = email;
        }
    }
}

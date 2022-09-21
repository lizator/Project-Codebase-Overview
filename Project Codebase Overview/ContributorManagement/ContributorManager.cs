using Project_Codebase_Overview.ContributorManagement.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ContributorManagement
{
    internal class ContributorManager
    {
        Dictionary<string, Author> Authors;
        private static ContributorManager Instance = null;

        private ContributorManager()
        {
            Authors = new Dictionary<string, Author>();
        }

        public static ContributorManager GetInstance()
        {
            if(Instance == null)
            {
                Instance = new ContributorManager();
            }
            return Instance;
        }



        private void AddAuthor(string email, string name)
        {
            Author author = new Author(email, name);
            this.Authors.Add(email, author);
        }

        public Author GetAuthor (string email)
        {
            return this.Authors[email];
        }

        public void InitializeAuthor(string email, string name)
        {
            if (this.Authors.ContainsKey(email))
            {
                this.Authors[email].AddAlias(name);
            } else
            {
                this.AddAuthor(email, name);
            }
        }
    }
}

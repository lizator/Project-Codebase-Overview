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
        Dictionary<string, Author> authors;

        private static ContributorManager instance = null;
        private int idCounter;
        private ContributorManager()
        {
            authors = new Dictionary<string, Author>();
            idCounter = 1;
        }

        public static ContributorManager getInstance()
        {
            if(instance == null)
            {
                instance = new ContributorManager();
            }
            return instance;
        }

        public void addAuthor(string email, string name)
        {
            Author author = new Author(idCounter, email, name);
            this.authors.Add(email, author);
            idCounter++;
        }

        public Author getAuthor (string email)
        {
            return this.authors[email];
        }

        public Author getOrCreateAuthor (string email, string name)
        {
            if (!authors.ContainsKey(email))
            {
                addAuthor(email, name);
            }
            return getAuthor(email);
        }
    }
}

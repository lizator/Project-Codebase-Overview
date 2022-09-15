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

        public static ContributorManager GetInstance()
        {
            if(instance == null)
            {
                instance = new ContributorManager();
            }
            return instance;
        }

        public void AddAuthor(string email, string name)
        {
            Author author = new Author(idCounter, email, name);
            this.authors.Add(email, author);
            idCounter++;
        }

        public Author GetAuthor (string email)
        {
            return this.authors[email];
        }

        public Author GetOrCreateAuthor (string email, string name)
        {
            if (!authors.ContainsKey(email))
            {
                AddAuthor(email, name);
            }
            return GetAuthor(email);
        }
    }
}

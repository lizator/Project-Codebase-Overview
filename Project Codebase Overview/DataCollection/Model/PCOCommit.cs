using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class PCOCommit
    {
        private int codeLines;
        private Author Author;
        private DateTime CommitDate;

        public PCOCommit(string email, DateTime commitDate)
        {
            this.codeLines = 0;
            this.Author = PCOState.GetInstance().GetContributorState().GetAuthor(email);
            this.CommitDate = commitDate;
        }

        public void AddLine(int amount = 1)
        {
            this.codeLines += amount;
        }
        
        public int GetLines()
        {
            return this.codeLines;
        }
        public Author GetAuthor()
        {
            return this.Author.OverAuthor == null ? this.Author : this.Author.OverAuthor;
        }

        public DateTime GetDate()
        {
            return this.CommitDate;
        }
    }
}

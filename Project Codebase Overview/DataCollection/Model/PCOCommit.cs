using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    internal class PCOCommit
    {
        private int codeLines;
        private int commentLines;
        private int whiteSpaceLines;
        private Author author;
        private DateTime commitDate;

        public PCOCommit(int codeLines, int commentLines, int whiteSpaceLines, string email, string name, DateTime commitDate)
        {
            this.codeLines = codeLines;
            this.commentLines = commentLines;
            this.whiteSpaceLines = whiteSpaceLines;
            this.author = ContributorManager.getInstance().getOrCreateAuthor(email, name);
            this.commitDate = commitDate;
        }
    }
}

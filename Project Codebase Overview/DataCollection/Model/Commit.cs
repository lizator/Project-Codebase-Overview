using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    internal class Commit
    {
        private int codeLines;
        private int commentLines;
        private int whiteSpaceLines;
        private object author;
        private DateTime commitDate;

        public Commit(int codeLines, int commentLines, int whiteSpaceLines, object author, DateTime commitDate)
        {
            this.codeLines = codeLines;
            this.commentLines = commentLines;
            this.whiteSpaceLines = whiteSpaceLines;
            this.author = author;
            this.commitDate = commitDate;
        }
    }
}

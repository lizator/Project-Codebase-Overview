using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
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
        private int commentLines;
        private int whiteSpaceLines;
        private Author author;
        private DateTime commitDate;

        public PCOCommit(int codeLines, int commentLines, int whiteSpaceLines, string email, string name, DateTime commitDate)
        {
            this.codeLines = codeLines;
            this.commentLines = commentLines;
            this.whiteSpaceLines = whiteSpaceLines;
            this.author = ContributorManager.GetInstance().GetAuthor(email);
            this.commitDate = commitDate;
        }

        public void AddLine(LineType type)
        {
            if (type == LineType.NORMAL)
            {
                this.codeLines++;
            } else if (type == LineType.COMMENT)
            {
                this.commentLines++;
            } else if (type == LineType.WHITE_SPACE)
            {
                this.whiteSpaceLines++;
            }
        }

        public enum LineType
        {
            NORMAL = 1,
            COMMENT = 2,
            WHITE_SPACE = 3,
        }
    }
}

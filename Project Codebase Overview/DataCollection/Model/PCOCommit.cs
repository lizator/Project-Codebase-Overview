﻿using Project_Codebase_Overview.ContributorManagement;
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
        private int commentLines;
        private int whiteSpaceLines;
        private Author author;
        private DateTime commitDate;

        public PCOCommit(string email, string name, DateTime commitDate)
        {
            this.codeLines = 0;
            this.commentLines = 0;
            this.whiteSpaceLines = 0;
            this.author = PCOState.GetInstance().GetContributorState().GetAuthor(email);
            this.commitDate = commitDate;
        }

        public void AddLine(LineType type, int amount = 1)
        {
            if (type == LineType.NORMAL)
            {
                this.codeLines += amount;
            } else if (type == LineType.COMMENT)
            {
                this.commentLines += amount;
            } else if (type == LineType.WHITE_SPACE)
            {
                this.whiteSpaceLines += amount;
            }
        }
        
        public int GetLines()
        {
            //TODO: CURRENT SETTINGS SHOULD CONSIDER WHETHER COMMENTlINES AND WHITESPACE LINES SHOULD BE CONSIDERED

            // if codelines and commentlines considered
            //return this.codeLines + this.commentLines;
           
            //if all lines are considered
            return this.codeLines + this.whiteSpaceLines + this.commentLines ;
        }
        public Author GetAuthor()
        {
            return this.author.OverAuthor == null ? this.author : this.author.OverAuthor;
        }


        public enum LineType
        {
            NORMAL = 1,
            COMMENT = 2,
            WHITE_SPACE = 3,
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    internal class PCOFile : IExplorerItem
    {
        public string name { get; set; }

        public object graphModel { get; set;}

        public Folder parent { get; set; }
        public List<PCOCommit> commits;

        public void calculateData()
        {
            throw new NotImplementedException();
        }

        public PCOFile(string name, Folder parent, List<PCOCommit> commits = null)
        {
            this.name = name;
            this.parent = parent;
            this.commits = commits ?? new List<PCOCommit>();
        }
    }
}

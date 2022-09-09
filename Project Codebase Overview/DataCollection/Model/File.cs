using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    internal class File : IExplorerItem
    {
        public string name { get; set; }

        public object graphModel => throw new NotImplementedException();

        public Folder parent { get; set; }
        public List<Commit> commits;

        public void calculateData()
        {
            throw new NotImplementedException();
        }

        public File(string name, Folder parent, List<Commit> commits = null)
        {
            this.name = name;
            this.parent = parent;
            this.commits = commits ?? new List<Commit>();
        }
    }
}

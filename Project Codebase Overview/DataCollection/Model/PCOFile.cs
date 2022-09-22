using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class PCOFile : IExplorerItem
    {
        public string Name { get; set; }

        public object GraphModel { get; set;}

        public PCOFolder Parent { get; set; }
        public List<PCOCommit> commits;

        public void CalculateData()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(PCOFile))
            {
                return 1;
            }
            return string.Compare(this.Name, ((PCOFile)obj).Name, StringComparison.InvariantCulture);
        }

        public PCOFile(string name, PCOFolder parent, List<PCOCommit> commits = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.commits = commits ?? new List<PCOCommit>();
        }
    }
}

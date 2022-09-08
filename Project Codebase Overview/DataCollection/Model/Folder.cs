using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    internal class Folder : IExplorerItem
    {
        public string name { get; set; }

        public object graphModel => throw new NotImplementedException();

        public Folder parent { get; set; }

        public Folder(string name, Folder parent)
        {
            this.name = name;
            this.parent = parent;
        }

        public void CalculateData()
        {
            throw new NotImplementedException();
        }

        public List<IExplorerItem> children => throw new NotImplementedException();
    }
}

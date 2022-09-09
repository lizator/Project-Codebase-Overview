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
        private List<IExplorerItem> children;

        public Folder(string name, Folder parent)
        {
            this.name = name;
            this.parent = parent;
            this.children = new List<IExplorerItem>();
        }

        public void calculateData()
        {
            throw new NotImplementedException();
        }


        public void addChild(IExplorerItem child)
        {
            children.Add(child);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class Folder : IExplorerItem, INotifyCollectionChanged
    {
        public string name { get; set; }
        public object graphModel { get; set; }
        public Folder parent { get; set; }
        public List<IExplorerItem> children { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

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
        public void addChildren(IExplorerItem[] child)
        {
            children.AddRange(child);
        }

    }
}

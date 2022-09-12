using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class PCOFolder : IExplorerItem, INotifyCollectionChanged
    {
        public string name { get; set; }
        public object graphModel { get; set; }
        public PCOFolder parent { get; set; }
        public List<IExplorerItem> children { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public PCOFolder(string name, PCOFolder parent)
        {
            this.name = name;
            this.parent = parent;
            this.children = new List<IExplorerItem>();
        }

        public void CalculateData()
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

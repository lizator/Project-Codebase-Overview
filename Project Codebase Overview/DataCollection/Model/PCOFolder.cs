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
        public Dictionary<string, IExplorerItem> children { get; } // <childname, childobject>

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public PCOFolder(string name, PCOFolder parent)
        {
            this.name = name;
            this.parent = parent;
            this.children = new Dictionary<string, IExplorerItem>();
        }

        public void CalculateData()
        {
            throw new NotImplementedException();
        }


        public void AddChild(IExplorerItem child)
        {
            children.Add(child.name, child);
        }
        public void AddChildren(IExplorerItem[] child)
        {
            throw new NotImplementedException();
            //children.AddRange(child);
        }

        public PCOFile AddChildRecursive(string[] list, int index)
        {
            if (list.Length == index + 1) //reached file
            {
                PCOFile newFile = new PCOFile(list[index], this);
                children.Add(newFile.name, newFile);
                return newFile;
            }
            else
            {
                if (!children.ContainsKey(list[index]))
                {
                    PCOFolder newFolder = new PCOFolder(list[index], this);
                    children.Add(newFolder.name, newFolder);
                }

                return ((PCOFolder)children[list[index]]).AddChildRecursive(list, index + 1);
            }
        }
    }
}

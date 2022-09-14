using Syncfusion.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(PCOFolder))
            {
                return -1;
            }
            return string.Compare(this.name, ((PCOFolder)obj).name, StringComparison.InvariantCulture);
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

        public List<IExplorerItem> SortedChildren { get => GetSortedChildren(); }

        private List<IExplorerItem> GetSortedChildren()
        {
            var x = this.children.Values.ToArray();
            Array.Sort(x);
            return x.ToList();
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

        public void AddChildrenAlternativly(List<string> filePaths, int index = 0)
        {
            var explorerGroups = filePaths.GroupBy(path => path.Split("/")[index]);

            foreach (var group in explorerGroups)
            {
                if (group.Count() == 1)
                { // a file
                    var file = new PCOFile(group.Key, this);
                    GitDataCollector.AddFileCommitsAlt(file, group.First());
                    this.AddChild(file);
                } else
                { // a folder
                    var folder = new PCOFolder(group.Key, this);
                    folder.AddChildrenAlternativly(group.ToList(), index + 1);
                    this.AddChild(folder);
                }

            }
        }
    }
}

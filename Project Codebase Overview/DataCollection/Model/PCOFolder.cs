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
        public string Name { get; set; }
        public GraphModel GraphModel { get; set; }
        public PCOFolder Parent { get; set; }
        public Dictionary<string, IExplorerItem> Children { get; } // <childname, childobject>

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public PCOFolder(string name, PCOFolder parent)
        {
            this.Name = name;
            this.Parent = parent;
            this.Children = new Dictionary<string, IExplorerItem>();
            this.GraphModel = new GraphModel();
        }
        public int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(PCOFolder))
            {
                return -1;
            }
            return string.Compare(this.Name, ((PCOFolder)obj).Name, StringComparison.InvariantCulture);
        }

        public void CalculateData()
        {
            foreach (var child in Children)
            {
                child.Value.CalculateData();
                this.GraphModel.AddLineDistributions(child.Value.GraphModel.LineDistribution);
                this.GraphModel.LinesTotal += child.Value.GraphModel.LinesTotal;
            }
            this.GraphModel.UpdateSuggestedOwner();
        }


        public void AddChild(IExplorerItem child)
        {
            Children.Add(child.Name, child);
        }
        public void AddChildren(IExplorerItem[] child)
        {
            throw new NotImplementedException();
            //children.AddRange(child);
        }

        public List<IExplorerItem> SortedChildren { get => GetSortedChildren(); }

        private List<IExplorerItem> GetSortedChildren()
        {
            var x = this.Children.Values.ToArray();
            Array.Sort(x);
            return x.ToList();
        }

        public PCOFile AddChildRecursive(string[] list, int index)
        {
            if (list.Length == index + 1) //reached file
            {
                PCOFile newFile = new PCOFile(list[index], this);
                Children.Add(newFile.Name, newFile);
                return newFile;
            }
            else
            {
                if (!Children.ContainsKey(list[index]))
                {
                    PCOFolder newFolder = new PCOFolder(list[index], this);
                    Children.Add(newFolder.Name, newFolder);
                }

                return ((PCOFolder)Children[list[index]]).AddChildRecursive(list, index + 1);
            }
        }

        public void AddChildrenAlternativly(List<string[]> filePaths, int index = 0)
        {
            var explorerGroups = filePaths.GroupBy(path => path[index]);

            foreach (var group in explorerGroups)
            {
                if (group.Count() == 1)
                { // a file
                    var file = new PCOFile(group.Key, this);
                    GitDataCollector.AddFileCommitsAlt(file, String.Join("/", group.First()));
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

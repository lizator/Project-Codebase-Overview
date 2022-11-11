using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.ContributorManagement;
using Syncfusion.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class PCOFolder : ExplorerItem, INotifyCollectionChanged
    {
        public Dictionary<string, ExplorerItem> Children { get; } // <childname, childobject>

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public PCOFolder(string name, PCOFolder parent)
        {
            this.Name = name;
            this.Parent = parent;
            this.Children = new Dictionary<string, ExplorerItem>();
            this.GraphModel = new GraphModel();
            this.GraphModel.FileName = name;
        }

        public override int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(PCOFolder))
            {
                return -1;
            }
            return string.Compare(this.Name, ((PCOFolder)obj).Name, StringComparison.InvariantCulture);
        }

        public override void CalculateData()
        {
            this.GraphModel = new GraphModel();
            this.GraphModel.FileName = Name;
            foreach (var child in Children)
            {
                child.Value.CalculateData();
                this.GraphModel.AddLineDistributions(child.Value.GraphModel.LineDistribution);
                this.GraphModel.LinesTotal += child.Value.GraphModel.LinesTotal;
            }
            this.GraphModel.UpdateSuggestedOwner();
            this.GenerateBarGraph();
        }


        public void AddChild(ExplorerItem child)
        {
            Children.Add(child.Name, child);
        }
        public void AddChildren(ExplorerItem[] child)
        {
            throw new NotImplementedException();
            //children.AddRange(child);
        }

        public List<ExplorerItem> SortedChildren { get => GetSortedChildren(); }
        public string LinesTotal { get => this.GraphModel.LinesTotal.ToString(); }

        private List<ExplorerItem> GetSortedChildren()
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
            // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
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

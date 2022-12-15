using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Gauges;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class PCOFolder : ExplorerItem, INotifyCollectionChanged
    {
        public Dictionary<string, ExplorerItem> Children { get; } // <childname, childobject>
        public Dictionary<string, ExplorerItem> ViewChildren { get; } // <childname, childobject>

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public PCOFolder(string name, PCOFolder parent)
        {
            this.Name = name;
            this.Parent = parent;
            this.Children = new Dictionary<string, ExplorerItem>();
            this.ViewChildren = new Dictionary<string, ExplorerItem>();
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
            this.ViewChildren.Clear();
            this.GraphModel = new GraphModel();
            this.GraphModel.FileName = Name;
            foreach (var child in Children)
            {
                child.Value.CalculateData();
                if(child.Value.SelectedOwners.Count > 0)
                {
                    this.GraphModel.AddLineDistributions_SelectedOwner(child.Value.GraphModel.LineDistribution, child.Value.SelectedOwners.ToList());   
                }
                else
                {
                    this.GraphModel.AddLineDistributions(child.Value.GraphModel.LineDistribution);
                }
                this.GraphModel.LinesTotal += child.Value.GraphModel.LinesTotal;
                this.GraphModel.LinesAfterDecay += child.Value.GraphModel.LinesAfterDecay;
                if (PCOState.GetInstance().GetSettingsState().IsFilesVisibile || child.Value.GetType() == typeof(PCOFolder))
                {
                    ViewChildren.Add(child.Key, child.Value);
                }
            }
            this.GraphModel.UpdateSuggestedOwner();
            //this.GenerateBarGraph();
        }

        public override string ToCodeowners()
        {
            var builder = new StringBuilder();

            var CodeownerLinesForThis = this.GetCodeownerLines();
            if (CodeownerLinesForThis.Count() > 0)
            {
                builder.Append(CodeownerLinesForThis);
            }

            foreach (var child in Children)
            {
                var txt = child.Value.ToCodeowners();
                if (txt.Length > 0)
                {
                    builder.Append(txt);
                }
            }

            return builder.ToString();
        }


        public void AddChild(ExplorerItem child)
        {
            Children.Add(child.Name, child);
        }

        public void RemoveChild(ExplorerItem child)
        {
            Children.Remove(child.Name);
        }
        public bool ContainsChild(ExplorerItem child)
        {
            return Children.ContainsKey(child.Name);
        }
        public void AddChildren(ExplorerItem[] child)
        {
            throw new NotImplementedException();
            //children.AddRange(child);
        }

        public List<ExplorerItem> SortedChildren { get => GetSortedChildren(); }

        private List<ExplorerItem> GetSortedChildren()
        {
            var x = this.Children.Values.ToArray();
            Array.Sort(x);
            return x.ToList();
        }

        public List<ExplorerItem> SortedViewChildren { get => GetSortedViewChildren(); }

        private List<ExplorerItem> GetSortedViewChildren()
        {
            var x = this.ViewChildren.Values.ToArray();
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

        public bool IsPathInitialized(string path)
        {
            var split = path.Split('/');
            if (split.Length == 1)
            {
                return this.Children.ContainsKey(split[0]) && this.Children[split[0]].GetType() == typeof(PCOFile);
            } else
            {
                if (this.Children.ContainsKey(split[0]) && this.Children[split[0]].GetType() == typeof(PCOFolder))
                {
                    var folder = (PCOFolder)this.Children[split[0]];
                    var newPath = path.Substring(path.IndexOf('/') + 1);
                    return folder.IsPathInitialized(newPath);
                }
                return false;
            }
        }

        public void RemoveFileByPath(string path)
        {
            var split = path.Split('/');
            if (split.Length == 1)
            {
                if(this.Children.ContainsKey(split[0]) && this.Children[split[0]].GetType() == typeof(PCOFile))
                {
                    RemoveChild(Children[split[0]]);
                }
                else
                {
                    Debug.WriteLine("Could not find file to remove in structure during update: " + split[0]);
                }
            }
            else
            {
                if (this.Children.ContainsKey(split[0]) && this.Children[split[0]].GetType() == typeof(PCOFolder))
                {
                    var folder = (PCOFolder)this.Children[split[0]];
                    var newPath = path.Substring(path.IndexOf('/') + 1);
                    folder.RemoveFileByPath(newPath);
                }
                else
                {
                    Debug.WriteLine("Could not find folder of file to remove in structure during update: " + path);
                }
            }
        }
    }
}

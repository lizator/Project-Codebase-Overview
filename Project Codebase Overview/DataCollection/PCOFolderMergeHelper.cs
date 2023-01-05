using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection
{
    internal class PCOFolderMergeHelper
    {

        public static PCOFolder MergeFolders(PCOFolder folderA, PCOFolder folderB)
        {
            if (folderA.Name != folderB.Name || folderA.Parent != folderB.Parent)
            {
                return null;
            }

            return Merge(folderA, folderB);
        }

        private static PCOFolder Merge(PCOFolder folderA, PCOFolder folderB)
        {
            foreach(ExplorerItem item in folderB.SortedChildren)
            {
                if (item is PCOFile)
                {
                    ObservableCollection<IOwner> selectedOwners = new ObservableCollection<IOwner>(); 
                    if (folderA.ContainsChild(item))
                    {
                        selectedOwners = folderA.Children[item.Name].SelectedOwners;
                        ((PCOFile)item).Creator = ((PCOFile)folderA.Children[item.Name]).Creator;
                        folderA.RemoveChild(item);
                    }
                    item.SelectedOwners = selectedOwners;
                    item.Parent = folderA;
                    folderA.AddChild(item);
                } else
                {
                    if (folderA.Children.ContainsKey(item.Name))
                    {
                        folderA.Children[item.Name] = Merge((PCOFolder)folderA.Children[item.Name], (PCOFolder)item);
                    } else
                    {
                        item.Parent = folderA;
                        folderA.AddChild(item);
                    }
                }
            }
            return folderA;
        }

    }
}

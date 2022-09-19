using Project_Codebase_Overview.DataCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection
{
    internal class PCOFolderMergeHelper
    {

        public static PCOFolder MergeFolders(PCOFolder folderA, PCOFolder folderB)
        {
            if (folderA.name != folderB.name || folderA.parent != folderB.parent)
            {
                return null;
            }


            return Merge(folderA, folderB);
        }

        private static PCOFolder Merge(PCOFolder folderA, PCOFolder folderB)
        {
            foreach(IExplorerItem item in folderB.SortedChildren)
            {
                if (item is PCOFile)
                {
                    folderA.AddChild(item);
                } else
                {
                    if (folderA.children.ContainsKey(item.name))
                    {
                        folderA.children[item.name] = Merge((PCOFolder)folderA.children[item.name], (PCOFolder)item);
                    } else
                    {
                        folderA.AddChild(item);
                    }
                }
            }
            return folderA;
        }

    }
}

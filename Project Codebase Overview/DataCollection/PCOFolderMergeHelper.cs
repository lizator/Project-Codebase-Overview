﻿using Project_Codebase_Overview.DataCollection.Model;
using System;
using System.Collections.Generic;
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

            Debug.WriteLine("Merge ran");

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
                    if (folderA.Children.ContainsKey(item.Name))
                    {
                        folderA.Children[item.Name] = Merge((PCOFolder)folderA.Children[item.Name], (PCOFolder)item);
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
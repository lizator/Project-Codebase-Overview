using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.TestDocs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.FileExplorerView
{
    public class ExplorerState
    {
        private PCOFolder RootFolder;
        private string RootPath;
        private List<PCOFolder> FolderHistory = new List<PCOFolder>();
        private int CurrentRooIndex;
        private static readonly int MAX_HISTORY_SIZE = 20;

        public ExplorerState()
        {

        }
        public void AddFolderHistory(PCOFolder addedFolder)
        {
            //remove forwardhistory
            FolderHistory = FolderHistory.GetRange(0, CurrentRooIndex + 1);

            //if max history reached remove start
            if (FolderHistory.Count == MAX_HISTORY_SIZE)
            {
                FolderHistory.RemoveAt(0);
                CurrentRooIndex -= 1;//index shifts left due to removal
            }

            FolderHistory.Add(addedFolder);
            CurrentRooIndex += 1;
        }
        public PCOFolder GetForwardHistoryFolder()
        {
            if (FolderHistory.Count > CurrentRooIndex + 1)
            {
                CurrentRooIndex += 1;
            }
            return FolderHistory[CurrentRooIndex];
        }

        public PCOFolder GetBackHistoryFolder()
        {
            if (CurrentRooIndex > 0)
            { 
                CurrentRooIndex -= 1;
            }
            return FolderHistory[CurrentRooIndex];
        }

        public void ResetHistory(PCOFolder newRoot)
        {
            FolderHistory.Clear();
            FolderHistory.Add(newRoot);
            CurrentRooIndex = 0;
        }
 
        public void SetRootPath(string path, bool forceReload = false)
        {
            if (!path.Equals(RootPath) || forceReload)
            {
                this.RootPath = path;
                LoadRootFolder();
                RootFolder.CalculateData();
            }
        }

        public string GetCurrentRootPath()
        {
            PCOFolder tempFolder = FolderHistory[CurrentRooIndex];
            string path = "";
            while (tempFolder.Parent != null)
            {
                path = tempFolder.Name + "\\" + path;
                tempFolder = tempFolder.Parent;
            }
            path = RootPath + "\\" + path;
            return path;
        }
        public string GetRootPath()
        {
            return this.RootPath;
        }

        public void LoadRootFolder(bool loadDummyData = false, bool withMerge = false)
        {
            if (loadDummyData)
            {
                this.RootPath = "C://SuperDummy/RootPath";
                if (withMerge)
                {
                    this.RootFolder = DummyDataSummoner.SummonDummyDataAndMerge();
                } else
                {
                    this.RootFolder = DummyDataSummoner.SummonDummyData();
                }
            } else
            {
                var collector = new GitDataCollector();
                try
                {
                    this.RootFolder = collector.CollectAllData(RootPath);
                    ResetHistory(this.RootFolder);
                } catch (Exception)
                {
                    throw;
                }
                
            }
        }

        public PCOFolder GetSubFolderFromPath(string path)
        {
            string relativePath = path.Substring(RootPath.Length + 1);
            var folderNames = relativePath.Split('\\');
            PCOFolder tempFolder = RootFolder;
            foreach(var folderName in folderNames)
            {
                tempFolder = (PCOFolder) tempFolder.Children.Where(folder => folder.Value.Name.Equals(folderName)).First().Value;
            }
            return tempFolder;
        }

        public PCOFolder GetRoot()
        {
            if (this.RootFolder != null)
            {
                return this.RootFolder;
            } else
            {
                throw new Exception("Root folder has not been loaded");
            }
        }

        public void TestSetRootFolder(PCOFolder folder)
        {
            this.RootFolder = folder;
            ResetHistory(folder);
        }
    }
}

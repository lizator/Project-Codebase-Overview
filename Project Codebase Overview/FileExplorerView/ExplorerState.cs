using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.State;
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
        private int CurrentRootIndex;
        private static readonly int MAX_HISTORY_SIZE = 20;

        public ExplorerState()
        {

        }
        public void AddFolderHistory(PCOFolder addedFolder)
        {
            //remove forwardhistory
            FolderHistory = FolderHistory.GetRange(0, CurrentRootIndex + 1);

            //if max history reached remove start
            if (FolderHistory.Count == MAX_HISTORY_SIZE)
            {
                FolderHistory.RemoveAt(0);
                CurrentRootIndex -= 1;//index shifts left due to removal
            }

            FolderHistory.Add(addedFolder);
            CurrentRootIndex += 1;
        }
        public PCOFolder GetForwardHistoryFolder()
        {
            if (FolderHistory.Count > CurrentRootIndex + 1)
            {
                CurrentRootIndex += 1;
            }
            return FolderHistory[CurrentRootIndex];
        }

        public PCOFolder GetBackHistoryFolder()
        {
            if (CurrentRootIndex > 0)
            { 
                CurrentRootIndex -= 1;
            }
            return FolderHistory[CurrentRootIndex];
        }

        public void ResetHistory(PCOFolder newRoot)
        {
            FolderHistory.Clear();
            FolderHistory.Add(newRoot);
            CurrentRootIndex = 0;
        }
 
        public void SetRootPath(string path)
        {
            this.RootPath = path;
           
        }

        public async Task<object> InitializeRoot()
        {
            await LoadRootFolder();
            RootFolder.CalculateData();
            PCOState.GetInstance().GetLoadingState().IsLoading = false;
            return null;
        }
        
        public PCOFolder GetCurrentRootFolder()
        {
            return FolderHistory[CurrentRootIndex];
        }

        public string GetCurrentRootPath()
        {
            PCOFolder tempFolder = FolderHistory[CurrentRootIndex];
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

        public async Task<object> LoadRootFolder(bool loadDummyData = false, bool withMerge = false)
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
                    var folder = await collector.CollectAllData(RootPath);
                    this.RootFolder = folder;
                    ResetHistory(this.RootFolder);
                } catch (Exception)
                {
                    throw;
                }
                
            }

            return null;
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

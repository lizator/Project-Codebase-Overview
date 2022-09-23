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

        public ExplorerState()
        {

        }

        public void SetRootPath(string path, bool forceReload = false)
        {
            if (!path.Equals(RootPath) || forceReload)
            {
                this.RootPath = path;
                LoadRootFolder();
                RootFolder.CalculateData();
                var ko = 20;
            }
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
                } catch (Exception)
                {
                    throw;
                }
                
            }
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

        public void SetRootFolder(PCOFolder folder)
        {
            this.RootFolder = folder;
        }
    }
}

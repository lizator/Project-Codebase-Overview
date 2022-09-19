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
        private PCOFolder root;
        private string rootPath;

        public ExplorerState()
        {

        }

        public void SetRootPath(string path, bool forceReload = false)
        {
            if (!path.Equals(rootPath) || forceReload)
            {
                this.rootPath = path;
                LoadRootFolder();
            }
        }

        public void LoadRootFolder(bool loadDummyData = false, bool withMerge = false)
        {
            if (loadDummyData)
            {
                if (withMerge)
                {
                    this.root = DummyDataSummoner.SummonDummyDataAndMerge();
                } else
                {
                    this.root = DummyDataSummoner.SummonDummyData();
                }
            } else
            {
                var collector = new GitDataCollector();
                try
                {
                    this.root = collector.CollectAllData(rootPath);
                } catch (Exception)
                {
                    throw;
                }
                
            }
        }

        public PCOFolder GetRoot()
        {
            if (this.root != null)
            {
                return this.root;
            } else
            {
                throw new Exception("Root folder has not been loaded");
            }
        }

        public void SetRoot(PCOFolder folder)
        {
            this.root = folder;
        }
    }
}

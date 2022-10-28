using CommunityToolkit.Mvvm.ComponentModel;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.State;
using Project_Codebase_Overview.TestDocs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.FileExplorerView
{
    public class ExplorerViewModel : ObservableObject
    {
        private PCOFolder viewRootFolder;
        public ExplorerItem SelectedGraphItem{
            get => _selectedGraphItem;
            set => SetProperty(ref _selectedGraphItem, value);
        }
        private ExplorerItem _selectedGraphItem;
        public string CurrentRootPath { 
            get => currentRootPath;
            set => SetProperty(ref currentRootPath, value); 
        }

        public ExplorerViewModel()
        {

        }

        private ObservableCollection<ExplorerItem> _explorerItems;
        private string currentRootPath;

        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get => _explorerItems;
            set => _explorerItems = value;
        }

        public void SetExplorerItems(PCOFolder root)
        {
            if (this.ExplorerItems == null)
            {
                this.ExplorerItems = new ObservableCollection<ExplorerItem>();
            }
            this.ExplorerItems.Clear();
            var explorerItems = new List<ExplorerItem>();
            foreach (var item in root.SortedChildren)
            {
                this.ExplorerItems.Add(item);
            }
            this.viewRootFolder = root;
            this.CurrentRootPath = PCOState.GetInstance().GetExplorerState().GetCurrentRootPath();
        }

        public void NavigateNewRoot(PCOFolder newFolder)
        {
            PCOState.GetInstance().GetExplorerState().AddFolderHistory(newFolder);
            SetExplorerItems(newFolder);
        }

        public void NavigateBack()
        {
            SetExplorerItems(PCOState.GetInstance().GetExplorerState().GetBackHistoryFolder());
        }

        public void NavigateForward()
        {
            SetExplorerItems(PCOState.GetInstance().GetExplorerState().GetForwardHistoryFolder());
        }

        public void NavigateUp()
        {
            if(viewRootFolder.Parent != null)
            {
                NavigateNewRoot(viewRootFolder.Parent);
            }
        }

        internal void NavigateToPath(string path)
        {
            NavigateNewRoot(PCOState.GetInstance().GetExplorerState().GetSubFolderFromPath(path));
        }
    }
}

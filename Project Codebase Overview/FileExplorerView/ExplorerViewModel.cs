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

        public NavigationButtonValues navButtonValues = new NavigationButtonValues();
        
        
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
            foreach (var item in root.SortedViewChildren)
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
            CheckNavigationOptions();
        }

        public void NavigateBack()
        {
            SetExplorerItems(PCOState.GetInstance().GetExplorerState().GetBackHistoryFolder());
            CheckNavigationOptions();
        }

        public void NavigateForward()
        {
            SetExplorerItems(PCOState.GetInstance().GetExplorerState().GetForwardHistoryFolder());
            CheckNavigationOptions();
        }

        public void NavigateUp()
        {
            if(viewRootFolder.Parent != null)
            {
                NavigateNewRoot(viewRootFolder.Parent);
            }
            CheckNavigationOptions();
        }

        internal void NavigateToPath(string path)
        {
            NavigateNewRoot(PCOState.GetInstance().GetExplorerState().GetSubFolderFromPath(path));
        }
        
        private void CheckNavigationOptions()
        {
            navButtonValues.NavigateUpAvailable = PCOState.GetInstance().GetExplorerState().IsNavigateUpAvailable();
            navButtonValues.NavigateBackAvailable = PCOState.GetInstance().GetExplorerState().IsNavigateBackAvailable();
            navButtonValues.NavigateForwardAvailable = PCOState.GetInstance().GetExplorerState().IsNavigateForwardAvailable();
        }
    }
    public class NavigationButtonValues : ObservableObject
    {
        public bool NavigateUpAvailable { get => _navigateUpAvailable; set => SetProperty(ref _navigateUpAvailable, value); }
        private bool _navigateUpAvailable = false;
        public bool NavigateBackAvailable { get => _navigateBackAvailable; set => SetProperty(ref _navigateBackAvailable, value); }
        private bool _navigateBackAvailable = false;
        public bool NavigateForwardAvailable { get => _navigateForwardAvailable; set => SetProperty(ref _navigateForwardAvailable, value); }
        private bool _navigateForwardAvailable = false;
    }
}

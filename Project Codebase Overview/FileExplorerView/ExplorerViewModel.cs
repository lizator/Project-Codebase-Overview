using CommunityToolkit.Mvvm.ComponentModel;
using LibGit2Sharp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.State;
using Project_Codebase_Overview.TestDocs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace Project_Codebase_Overview.FileExplorerView
{
    public class ExplorerViewModel : ObservableObject
    {

        public delegate void NotifyGraphUpdate();
        public event NotifyGraphUpdate NotifyGraphUpdateEvent;
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

        private BreadcrumbBar _pathBreadcrumbBar;
        public BreadcrumbBar PathBreadcrumbBar { get => _pathBreadcrumbBar; set => SetProperty(ref _pathBreadcrumbBar, value); }

        public ObservableCollection<PCOFolder> BreadcrumbBarFolderList = new ObservableCollection<PCOFolder>();

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
            UpdateBreadcrumbBar();
            NotifyGraphUpdateEvent?.Invoke();
        }

        public void NavigateBack()
        {
            SetExplorerItems(PCOState.GetInstance().GetExplorerState().GetBackHistoryFolder());
            CheckNavigationOptions();
            UpdateBreadcrumbBar();
        }

        public void NavigateForward()
        {
            SetExplorerItems(PCOState.GetInstance().GetExplorerState().GetForwardHistoryFolder());
            CheckNavigationOptions();
            UpdateBreadcrumbBar();
        }

        public void NavigateUp()
        {
            if(viewRootFolder.Parent != null)
            {
                NavigateNewRoot(viewRootFolder.Parent);
            }
            CheckNavigationOptions();
            UpdateBreadcrumbBar();
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

        public void UpdateBreadcrumbBar()
        {
            BreadcrumbBarFolderList.Clear();
            var currentFolder = PCOState.GetInstance().GetExplorerState().GetCurrentRootFolder();
            var stack = new Stack<PCOFolder>();
            do
            {
                stack.Push(currentFolder);
                currentFolder = currentFolder.Parent;
            } while (currentFolder != null);
            while (stack.Count > 0)
            {
                BreadcrumbBarFolderList.Add(stack.Pop());
            }

            var bar = new BreadcrumbBar();
            bar.ItemsSource = BreadcrumbBarFolderList.Select(x => x.Name).ToArray();
            bar.ItemClicked += PathBreadcrumbBar_ItemClicked;
            PathBreadcrumbBar = bar;

        }

        private void PathBreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            NavigateNewRoot(BreadcrumbBarFolderList[args.Index]);
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

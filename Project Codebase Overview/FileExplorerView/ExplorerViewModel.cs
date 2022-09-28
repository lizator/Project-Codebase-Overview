using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.DataCollection.Model;
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
    public class ExplorerViewModel
    {
        private PCOFolder viewRootFolder;

        public ExplorerViewModel() // TODO add path?
        {

        }

        private ObservableCollection<ExplorerItem> _explorerItems;

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
        }

        public PCOFolder GetViewRootFolder()
        {
            return this.viewRootFolder;
        }
    }
}

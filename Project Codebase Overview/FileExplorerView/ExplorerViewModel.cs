using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.TestDocs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.FileExplorerView
{
    public class ExplorerViewModel
    {
        public ExplorerViewModel() // TODO add path?
        {

        }

        private ObservableCollection<IExplorerItem> _explorerItems;

        public ObservableCollection<IExplorerItem> ExplorerItems
        {
            get => _explorerItems;
            set => _explorerItems = value;
        }

        public void SetExplorerItems(PCOFolder root)
        {
            var explorerItems = new ObservableCollection<IExplorerItem>();
            foreach (var item in root.SortedChildren)
            {
                explorerItems.Add(item);
            }
            this.ExplorerItems = explorerItems;
        }
    }
}

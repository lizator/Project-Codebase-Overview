using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ChangeHistory
{
    internal class OwnerChange : IChangeObject
    {
        IOwner PreviousOwner;
        IOwner NewOwner;
        ExplorerItem Item;

        public OwnerChange (IOwner prevOwner, IOwner newOwner, ExplorerItem item)
        {
            PreviousOwner = prevOwner;
            NewOwner = newOwner;
            Item = item;
        }

        public void RedoChange()
        {
            
        }

        public void UndoChange()
        {
            Item.GraphModel.SelectedOwner = PreviousOwner;
            
        }
    }
}

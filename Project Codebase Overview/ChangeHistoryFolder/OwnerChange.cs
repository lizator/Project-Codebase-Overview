using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Syncfusion.UI.Xaml.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ChangeHistoryFolder
{
    internal class OwnerChange : IChangeObject
    {
        IOwner PreviousOwner;
        IOwner NewOwner;
        ExplorerItem Item;
        SfComboBox SfComboBox;

        public OwnerChange (IOwner prevOwner, IOwner newOwner, ExplorerItem item, SfComboBox combobox)
        {
            PreviousOwner = prevOwner;
            NewOwner = newOwner;
            Item = item;
            SfComboBox = combobox;
        }

        public void RedoChange()
        {
            Item.GraphModel.SelectedOwner = NewOwner;
            Item.SelectedOwnerColor = null;
            Item.SelectedOwnerName = null;
            SfComboBox.Text = Item.SelectedOwnerName;
        }

        public void UndoChange()
        {
            Item.GraphModel.SelectedOwner = PreviousOwner;
            Item.SelectedOwnerColor = null;
            Item.SelectedOwnerName = null;
            if (Item.SelectedOwnerName.Equals("Unselected")){
                SfComboBox.Text = "";
            }
            else
            {
                SfComboBox.Text = Item.SelectedOwnerName;
            }   
        }
    }
}

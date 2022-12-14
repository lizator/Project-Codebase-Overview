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
        IOwner RemovedOwner;
        IOwner AddedOwner;
        ExplorerItem Item;
        SfComboBox SfComboBox;

        public OwnerChange (IOwner removedOwner, IOwner addedOwner, ExplorerItem item, SfComboBox combobox)
        {
            RemovedOwner = removedOwner;
            AddedOwner = addedOwner;
            Item = item;
            SfComboBox = combobox;
        }

        public void RedoChange()
        {
            if (AddedOwner != null)
            {
                if (AddedOwner.GetType() == typeof(Author))
                {
                    if (!Item.SelectedOwners.Where(o => o.GetType() == typeof(Author) && ((Author)o).Email.Equals(((Author)AddedOwner).Email)).Any())
                    {
                        Item.SelectedOwners.Add(AddedOwner);
                        SfComboBox.SelectedItems.Add(AddedOwner);
                    }
                } 
                else if (AddedOwner.GetType() == typeof(PCOTeam))
                {
                    if (!Item.SelectedOwners.Where(o => o.GetType() == typeof(PCOTeam) && o.Name.Equals(AddedOwner.Name)).Any())
                    {
                        Item.SelectedOwners.Add(AddedOwner);
                        SfComboBox.SelectedItems.Add(AddedOwner);
                    }
                }
            }
            else if (RemovedOwner != null)
            {
                if (RemovedOwner.GetType() == typeof(Author))
                {
                    if (Item.SelectedOwners.Where(o => o.GetType() == typeof(Author) && ((Author)o).Email.Equals(((Author)RemovedOwner).Email)).Any())
                    {
                        Item.SelectedOwners.Remove(RemovedOwner);
                        SfComboBox.SelectedItems.Remove(RemovedOwner);
                    }
                }
                else if (RemovedOwner.GetType() == typeof(PCOTeam))
                {
                    if (Item.SelectedOwners.Where(o => o.GetType() == typeof(PCOTeam) && o.Name.Equals(RemovedOwner.Name)).Any())
                    {
                        Item.SelectedOwners.Remove(RemovedOwner);
                        SfComboBox.SelectedItems.Remove(RemovedOwner);
                    }
                }
            }
        }

        public void UndoChange()
        {
            if (AddedOwner != null)
            {
                if (AddedOwner.GetType() == typeof(Author))
                {
                    if (Item.SelectedOwners.Where(o => o.GetType() == typeof(Author) && ((Author)o).Email.Equals(((Author)AddedOwner).Email)).Any())
                    {
                        Item.SelectedOwners.Remove(AddedOwner);
                        SfComboBox.SelectedItems.Remove(AddedOwner);
                    }
                }
                else if (AddedOwner.GetType() == typeof(PCOTeam))
                {
                    if (Item.SelectedOwners.Where(o => o.GetType() == typeof(PCOTeam) && o.Name.Equals(AddedOwner.Name)).Any())
                    {
                        Item.SelectedOwners.Remove(AddedOwner);
                        SfComboBox.SelectedItems.Remove(AddedOwner);
                    }
                }
            }
            else if (RemovedOwner != null)
            {
                if (RemovedOwner.GetType() == typeof(Author))
                {
                    if (!Item.SelectedOwners.Where(o => o.GetType() == typeof(Author) && ((Author)o).Email.Equals(((Author)RemovedOwner).Email)).Any())
                    {
                        Item.SelectedOwners.Add(RemovedOwner);
                        SfComboBox.SelectedItems.Add(RemovedOwner);
                    }
                }
                else if (RemovedOwner.GetType() == typeof(PCOTeam))
                {
                    if (!Item.SelectedOwners.Where(o => o.GetType() == typeof(PCOTeam) && o.Name.Equals(RemovedOwner.Name)).Any())
                    {
                        Item.SelectedOwners.Add(RemovedOwner);
                        SfComboBox.SelectedItems.Add(RemovedOwner);
                    }
                }
            }
        }
    }
}

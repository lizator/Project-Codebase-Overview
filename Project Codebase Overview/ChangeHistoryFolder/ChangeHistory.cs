using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ChangeHistoryFolder
{
    public class ChangeHistory : ObservableObject
    {
        List<IChangeObject> Changes;
        int CurrentUndoIndex;//Always points to the change you can UNDO at this point!
        readonly int MAX_CHANGE_HISTORY = 30;

        public bool RedoAvailable { get => _redoAvailable; set => SetProperty(ref _redoAvailable, value); }
        private bool _redoAvailable = false;
        public bool UndoAvailable { get => _undoAvailable; set => SetProperty(ref _undoAvailable, value); }
        private bool _undoAvailable = false;


        public ChangeHistory()
        {
            Changes = new List<IChangeObject>();
            CurrentUndoIndex = -1;
        }

        public void AddChange(IChangeObject change)
        { 
            //remove any forward history (redo)
            Changes = Changes.GetRange(0, CurrentUndoIndex + 1);

            //if max reached, remove start first item
            if(Changes.Count() == MAX_CHANGE_HISTORY)
            {
                Changes.RemoveAt(0);
                CurrentUndoIndex--;
            }

            //add item
            Changes.Add(change);
            CurrentUndoIndex++;

            UndoAvailable = true;
            RedoAvailable = false;
        }
        public void Undo()
        {
            if(CurrentUndoIndex >= 0)
            {
                Changes[CurrentUndoIndex].UndoChange();
                CurrentUndoIndex--;
                RedoAvailable = true;
            }
            if(CurrentUndoIndex < 0)
            {
                UndoAvailable = false;
            }
            
        }
        public void Redo()
        {
            if(CurrentUndoIndex < Changes.Count() - 1)
            {
                CurrentUndoIndex++;
                Changes[CurrentUndoIndex].RedoChange();
                UndoAvailable = true;
            }
            if(CurrentUndoIndex >= Changes.Count() - 1)
            {
                RedoAvailable = false;
            }
        }

    }
}

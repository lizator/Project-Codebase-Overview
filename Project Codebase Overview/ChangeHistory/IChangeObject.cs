using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ChangeHistory
{
    internal interface IChangeObject
    {
        void UndoChange();
        void RedoChange();
        
    }
}

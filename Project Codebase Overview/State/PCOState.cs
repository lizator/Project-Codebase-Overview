using Project_Codebase_Overview.FileExplorerView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.State
{
    public class PCOState
    {
        private ExplorerState ExplorerState;
        private static PCOState Instance;

        public static PCOState GetInstance()
        {
            if (Instance == null)
            {
               Instance = new PCOState();
            }
            return Instance;
        }

        private PCOState()
        {
            ExplorerState = new ExplorerState();
        }

        public ExplorerState GetExplorerState()
        {
            return ExplorerState;
        }
    }
}

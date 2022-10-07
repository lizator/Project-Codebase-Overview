using LibGit2Sharp;
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
        private LoadingState LoadingState;
        private TestState TestState;
        private static PCOState Instance;

        public Repository TempGitRepo;
        public int mergeCounter;

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
            LoadingState = new LoadingState();
            TestState = new TestState();
        }

        public ExplorerState GetExplorerState()
        {
            return ExplorerState;
        }

        public LoadingState GetLoadingState()
        {
            return LoadingState;
        }

        public TestState GetTestState()
        {
            return TestState;
        }

        public void ClearState()
        {
            ExplorerState = new ExplorerState();
            LoadingState = new LoadingState();
            TestState = new TestState();
        }
    }
}

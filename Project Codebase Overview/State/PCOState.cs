using LibGit2Sharp;
using Project_Codebase_Overview.ChangeHistoryFolder;
using Project_Codebase_Overview.ContributorManagement;
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
        public ChangeHistory ChangeHistory = new ChangeHistory();

        private ExplorerState ExplorerState;
        private LoadingState LoadingState;
        private TestState TestState;
        private SettingsState SettingsState;
        private ContributorState ContributorState;

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
            SettingsState = new SettingsState();
            ContributorState = new ContributorState();
        }
        public SettingsState GetSettingsState()
        {
            return SettingsState;
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

        public ContributorState GetContributorState()
        {
            return ContributorState;
        }

        public void ClearState()
        {
            ExplorerState = new ExplorerState();
            LoadingState = new LoadingState();
            TestState = new TestState();
            SettingsState = new SettingsState();
            ContributorState = new ContributorState();
            PCOColorPicker.ResetInstance();
        }
    }
}

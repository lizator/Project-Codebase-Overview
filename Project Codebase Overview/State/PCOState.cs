using LibGit2Sharp;
using Newtonsoft.Json;
using Project_Codebase_Overview.ChangeHistoryFolder;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.FileExplorerView;
using Project_Codebase_Overview.SaveState;
using Project_Codebase_Overview.SaveState.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

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
        private string latestCommitSha;
        

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
            //TODO: make a reset state for all the other shit (aldready done for explorerState
            LoadingState = new LoadingState();
            TestState = new TestState();
            SettingsState = new SettingsState();
            ContributorState = new ContributorState();
            PCOColorPicker.ResetInstance();
        }

        public void SetLatestCommitSha(string sha)
        {
            latestCommitSha = sha;
        }
        public string GetLatestCommitSha()
        {
            return latestCommitSha;
        }
        public async Task SaveStateToFile(StorageFile file)
        {
            var serializableState = SerializerHelper.GetSerializableStateFromPCOState(this);



            //var jsonString = System.Text.Json.JsonSerializer.Serialize(serializableState);
            var jsonString = JsonConvert.SerializeObject(serializableState, Formatting.Indented);
            await FileIO.WriteTextAsync(file, jsonString);
        }

        public void LoadFile(StorageFile file)
        {
            var path =  file.Path;
            var jsonString = File.ReadAllText(path);
            SerializerState serializerState = JsonConvert.DeserializeObject<SerializerState>(jsonString);

            SerializerHelper.SetPCOStateFromInitializerState(serializerState);
  
        }
    }
}

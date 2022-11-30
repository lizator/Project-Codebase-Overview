using LibGit2Sharp;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using Project_Codebase_Overview.ChangeHistoryFolder;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.FileExplorerView;
using Project_Codebase_Overview.LocalSettings;
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
        private string LatestCommitSha;
        private string BranchName;
        public Repository TempGitRepo;
        private bool CodeOwnersExportToTeamMissingID;



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
            ChangeHistory = new ChangeHistory();
        }

        public void SetLatestCommitSha(string sha)
        {
            LatestCommitSha = sha;
        }
        public string GetLatestCommitSha()
        {
            return LatestCommitSha;
        }

        public void SetBranchName(string name)
        {
            BranchName = name;
        }
        public string GetBranchName()
        {
            return BranchName;
        }
        public async Task SaveStateToFile(StorageFile file)
        {
            var serializableState = SerializerHelper.GetSerializableStateFromPCOState(this);



            //var jsonString = System.Text.Json.JsonSerializer.Serialize(serializableState);
            var jsonString = JsonConvert.SerializeObject(serializableState, Formatting.Indented);
            await FileIO.WriteTextAsync(file, jsonString);

            LocalSettingsHelper.AddRecentFile(file.Name, GetExplorerState().GetRoot().Name, file.Path);
        }
        public async Task ExportStateToCodeowners(StorageFile file)
        {
            CodeOwnersExportToTeamMissingID = false;

            var builder = new StringBuilder();
            builder.AppendLine("# Codeowners file created using PCO for project at \"" + this.ExplorerState.GetRootPath() + "\"");
            builder.AppendLine("# Created " + DateTime.Now.ToShortDateString());
            builder.AppendLine("#");
            builder.AppendLine("# This file should keep the name as purely \"CODEOWNERS\".");
            builder.AppendLine("# Place this file in the repository root directory or in the repositories .github/ or .gitlab/ folder.");
            builder.AppendLine("#");
            builder.AppendLine("# View the documentation for more information: https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners");
            builder.AppendLine("");
            builder.AppendLine("");
            builder.AppendLine("# ______ Codeowners start ______");

            builder.Append(this.ExplorerState.GetRoot().ToCodeowners());

            builder.AppendLine("");
            builder.AppendLine("# ______ Codeowners end ______");

            await FileIO.WriteTextAsync(file, builder.ToString());
        }

        public void SetCodeOwnersExportToTeamMissingID(bool val)
        {
            CodeOwnersExportToTeamMissingID = val;
        }
        public bool GetCodeOwnersExportToTeamMissingID()
        {
            return CodeOwnersExportToTeamMissingID;
        }

        public async Task<bool> LoadFile(StorageFile file)
        {
            var path =  file.Path;
            var jsonString = File.ReadAllText(path);
            SerializerState serializerState = JsonConvert.DeserializeObject<SerializerState>(jsonString);
            //setup state for old root
            SerializerHelper.SetPCOStateFromInitializerState(serializerState);
            LatestCommitSha = serializerState.LatestCommitSHA;
            //TODO: handle branching???

            //check if any new commits to load
            GitDataCollector gitDataCollector = new GitDataCollector();
            var repoChangesAvailable = gitDataCollector.IsRepoChangesAvailable(serializerState.RepositoryRootPath, serializerState.LatestCommitSHA);

            LocalSettingsHelper.AddRecentFile(file.Name, GetExplorerState().GetRoot().Name, file.Path);

            return repoChangesAvailable;
  
        }
    }
}

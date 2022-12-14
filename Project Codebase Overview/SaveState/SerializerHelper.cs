using Microsoft.UI.Xaml.Controls;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.FileExplorerView;
using Project_Codebase_Overview.SaveState.Model;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace Project_Codebase_Overview.SaveState
{
    public class SerializerHelper
    {
        public static SerializerState GetSerializableStateFromPCOState(PCOState pCOState)
        {
            // MAIN METHOD FOR CONVERTING PCOSTATE TO SERIALIZERSTATE
            SerializerState serialState = new SerializerState();

            serialState.RootFolder = GetSerializerRoot(pCOState.GetExplorerState().GetRoot());
            serialState.Teams = GetSerializerTeams(pCOState.GetContributorState().GetAllTeams());
            serialState.Authors = GetSerializerAuthors(pCOState.GetContributorState().GetAllAuthors());
            serialState.Settings = GetSerializerSettings(pCOState.GetSettingsState());
            serialState.RepositoryRootPath = pCOState.GetExplorerState().GetRootPath();
            serialState.LatestCommitSHA = pCOState.GetLatestCommitSha();
            serialState.BranchName = pCOState.GetBranchName();

            return serialState;
        }

        private static SerializerSettings GetSerializerSettings(SettingsState settingsState)
        {
            SerializerSettings serialSettings = new SerializerSettings();
            serialSettings.DecayTimeUnit = settingsState.DecayTimeUnit;
            serialSettings.IsDecayActive = settingsState.IsDecayActive;
            serialSettings.DecayDropOffInteval = settingsState.DecayDropOffInteval;
            serialSettings.DecayPercentage = settingsState.DecayPercentage;
            serialSettings.CreatorMultiplier = settingsState.CreatorBonusPercent;

            return serialSettings;
        }

        private static List<SerializerAuthor> GetSerializerAuthors(List<Author> pCOAuthors)
            // (pCOAuthors from getAllAuthors are all parent-authors !
        {
            List<SerializerAuthor> serializerAuthors = new List<SerializerAuthor>();
            foreach(var pCOAuthor in pCOAuthors)
            {
                SerializerAuthor serialAuthor = new SerializerAuthor();
                serialAuthor.Name = pCOAuthor.Name;
                serialAuthor.Email = pCOAuthor.Email;
                serialAuthor.VCSEmail = pCOAuthor.VCSEmail;
                serialAuthor.Color= pCOAuthor.Color;
                serialAuthor.Aliases = pCOAuthor.Aliases.ToList();
                serialAuthor.IsActive = pCOAuthor.IsActive;

                serialAuthor.SubAuthors = new List<SerializerAuthor>();
                foreach(var pCOSubAuthor in pCOAuthor.SubAuthors)
                {
                    SerializerAuthor serialSubAuthor = new SerializerAuthor();
                    serialSubAuthor.Name = pCOSubAuthor.Name;
                    serialSubAuthor.Email = pCOSubAuthor.Email;
                    serialSubAuthor.VCSEmail = pCOSubAuthor.VCSEmail;
                    serialSubAuthor.Color = pCOSubAuthor.Color;
                    serialSubAuthor.Aliases = pCOSubAuthor.Aliases.ToList();
                    serialSubAuthor.IsActive = serialSubAuthor.IsActive;

                    serialAuthor.SubAuthors.Add(serialSubAuthor);
                }

                serializerAuthors.Add(serialAuthor);
            }
            return serializerAuthors;
        }

        private static List<SerializerTeam> GetSerializerTeams(List<PCOTeam> pCOTeams)
        {
            List<SerializerTeam> serialTeams = new List<SerializerTeam>();

            foreach(var pCOTeam in pCOTeams)
            {
                SerializerTeam serialTeam = new SerializerTeam();
                serialTeam.Name = pCOTeam.Name;
                serialTeam.Color = pCOTeam.Color;
                serialTeam.MemberEmails = pCOTeam.Members.Select(x => x.Email).ToList();
                serialTeam.VCSID = pCOTeam.VCSID;

                serialTeams.Add(serialTeam);
            }
            return serialTeams;
        }

        private static SerializerFolder GetSerializerRoot(PCOFolder pCOFolder)
        {
            SerializerFolder serialFolder = new SerializerFolder();
            serialFolder.Name = pCOFolder.Name;
            serialFolder.IsActive = pCOFolder.IsActive;
            serialFolder.Comment = pCOFolder.Comment ?? "";

            serialFolder.SelectedTeamNames = pCOFolder.SelectedOwners.Where(owner => owner.GetType() == typeof(PCOTeam)).Select(owner => owner.Name).ToList();
            serialFolder.SelectedAuthorEmails = pCOFolder.SelectedOwners.Where(owner => owner.GetType() == typeof(Author)).Select(owner => ((Author)owner).Email).ToList();

            foreach (var child in pCOFolder.Children.Values)
            {
                if(child.GetType() == typeof(PCOFolder))
                {
                    serialFolder.SubFolders.Add(GetSerializerRoot(child as PCOFolder));
                }
                else
                {
                    serialFolder.SubFiles.Add(GetSerializerFile(child as PCOFile));
                }
            }
            
            return serialFolder;
        }
        private static SerializerFile GetSerializerFile(PCOFile pCOFile)
        {
            SerializerFile serialFile = new SerializerFile();
            serialFile.Name = pCOFile.Name;
            serialFile.IsActive = pCOFile.IsActive;
            serialFile.CreatorEmail = pCOFile.Creator?.Email ?? "";
            serialFile.Comment = pCOFile.Comment ?? "";

            serialFile.SelectedTeamNames = pCOFile.SelectedOwners.Where(owner => owner.GetType() == typeof(PCOTeam)).Select(owner => owner.Name).ToList();
            serialFile.SelectedAuthorEmails = pCOFile.SelectedOwners.Where(owner => owner.GetType() == typeof(Author)).Select(owner => ((Author)owner).Email).ToList();

            foreach (var commit in pCOFile.commits)
            {
                SerializerCommit serialCommit = new SerializerCommit();
                serialCommit.AuthorEmail = commit.GetAuthor().Email;
                serialCommit.LineCount = commit.GetLines();
                serialCommit.Date = commit.GetDate();
                serialFile.Commits.Add(serialCommit);
            }

            return serialFile;
        }

        public static async void SetPCOStateFromInitializerState(SerializerState serializerState)
        {
            // MAIN METHOD FOR CONVERTING (AND LOADING) FROM SERIALIZERSTATE TO PCOSTATE
            PCOState.GetInstance().ClearState();

            PCOState.GetInstance().SetBranchName(serializerState.BranchName);
            SetPCOAuthors(serializerState);
            SetPCOTeams(serializerState);
            SetPCOSettings(serializerState.Settings);

            var oldDataRoot = GetPCORoot(serializerState.RootFolder, null);
            PCOState.GetInstance().GetExplorerState().ResetState(oldDataRoot, serializerState.RepositoryRootPath);

            
        }

        private static void SetPCOAuthors(SerializerState serializerState)
        {
            var contributorState = PCOState.GetInstance().GetContributorState();

            foreach(var serialAuthor in serializerState.Authors)
            {
                //create and get author
                contributorState.InitializeAuthor(serialAuthor.Email, serialAuthor.Name, serialAuthor.Color);
                var pcoAuthor = contributorState.GetAuthor(serialAuthor.Email);
                pcoAuthor.VCSEmail = serialAuthor.VCSEmail;
                pcoAuthor.IsActive = serialAuthor.IsActive;
                //Add aliases
                foreach(var alias in serialAuthor.Aliases)
                {
                    pcoAuthor.AddAlias(alias);
                }
                //add and connect subauthors
                foreach(var serialSubAuthor in serialAuthor.SubAuthors)
                {
                    contributorState.InitializeAuthor(serialSubAuthor.Email, serialSubAuthor.Name, serialSubAuthor.Color);
                    var subAuthor = contributorState.GetAuthor(serialSubAuthor.Email);
                    subAuthor.VCSEmail = serialSubAuthor.VCSEmail;
                    subAuthor.IsActive = serialSubAuthor.IsActive;
                    pcoAuthor.ConnectAuthor(subAuthor);
                }
            }
        }

        private static void SetPCOTeams(SerializerState serializerState)
        {
            var contributorState = PCOState.GetInstance().GetContributorState();

            foreach(var serialTeam in serializerState.Teams)
            {
                PCOTeam pcoTeam = new PCOTeam(serialTeam.Name, serialTeam.Color);
                pcoTeam.VCSID = serialTeam.VCSID;
                
                foreach(var memberEmail in serialTeam.MemberEmails)
                {
                    pcoTeam.ConnectMember(contributorState.GetAuthor(memberEmail));
                }
                contributorState.AddTeam(pcoTeam);
            }
        }

        private static void SetPCOSettings(SerializerSettings serialSettings)
        {
            var settingsState = PCOState.GetInstance().GetSettingsState();
            settingsState.IsDecayActive = serialSettings.IsDecayActive;
            settingsState.DecayTimeUnit = serialSettings.DecayTimeUnit;
            settingsState.DecayDropOffInteval = serialSettings.DecayDropOffInteval;
            settingsState.DecayPercentage = serialSettings.DecayPercentage;
            settingsState.CreatorBonusPercent = serialSettings.CreatorMultiplier;
        }

        private static PCOFolder GetPCORoot(SerializerFolder serialFolder, PCOFolder parent)
        {
            
            //create the folder
            PCOFolder pCOFolder = new PCOFolder(serialFolder.Name, parent);
            pCOFolder.IsActive = serialFolder.IsActive;
            pCOFolder.Comment = serialFolder.Comment ?? "";
            //set owner
            if (serialFolder.SelectedAuthorEmails?.Count > 0)
            {
                foreach (var authorEmail in serialFolder.SelectedAuthorEmails)
                {
                    if (!pCOFolder.SelectedOwners.Where(a => a.GetType() == typeof(Author) && ((Author)a).Email.Equals(authorEmail)).Any())
                    {
                        pCOFolder.SelectedOwners.Add(PCOState.GetInstance().GetContributorState().GetAuthor(authorEmail));
                    }
                }
            }
            if (serialFolder.SelectedTeamNames?.Count > 0)
            {
                foreach (var teamName in serialFolder.SelectedTeamNames)
                {
                    if (!pCOFolder.SelectedOwners.Where(t => t.GetType() == typeof(PCOTeam) && ((PCOTeam)t).Name.Equals(teamName)).Any())
                    {
                        pCOFolder.SelectedOwners.Add(PCOState.GetInstance().GetContributorState().GetAllTeams().Find(x => x.Name.Equals(teamName)));
                    }
                }
            }
            //add subfolders to the folder
            foreach (var subFolder in serialFolder.SubFolders)
            {
                pCOFolder.AddChild(GetPCORoot(subFolder, pCOFolder));
            }
            
            //add subfiles to the folder
            foreach(var subFile in serialFolder.SubFiles)
            {
                PCOFile pCOFile = new PCOFile(subFile.Name, pCOFolder);
                pCOFile.IsActive = subFile.IsActive;
                pCOFile.Comment = subFile.Comment ?? "";

                pCOFile.Creator = !subFile.CreatorEmail.Equals("") ? PCOState.GetInstance().GetContributorState().GetAuthor(subFile.CreatorEmail) : null;
                if (subFile.SelectedAuthorEmails?.Count > 0)
                {
                    foreach (var authorEmail in subFile.SelectedAuthorEmails)
                    {
                        if (!pCOFile.SelectedOwners.Where(a => a.GetType() == typeof(Author) && ((Author)a).Email.Equals(authorEmail)).Any())
                        {
                            pCOFile.SelectedOwners.Add(PCOState.GetInstance().GetContributorState().GetAuthor(authorEmail));
                        }
                    }
                }
                if (subFile.SelectedTeamNames?.Count > 0)
                {
                    foreach (var teamName in subFile.SelectedTeamNames)
                    {
                        if (!pCOFile.SelectedOwners.Where(t => t.GetType() == typeof(PCOTeam) && ((PCOTeam)t).Name.Equals(teamName)).Any())
                        {
                            pCOFile.SelectedOwners.Add(PCOState.GetInstance().GetContributorState().GetAllTeams().Find(x => x.Name.Equals(teamName)));
                        }
                    }
                }
                foreach (var serialCommit in subFile.Commits)
                {
                    PCOCommit pCOCommit = new PCOCommit(serialCommit.AuthorEmail, serialCommit.Date);
                    pCOCommit.AddLine(serialCommit.LineCount);
                    pCOFile.commits.Add(pCOCommit);
                }
                pCOFolder.AddChild(pCOFile);
            }
            
            return pCOFolder;
        }
    }
}

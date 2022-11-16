using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.SaveState.Model;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState
{
    public class SerializerHelper
    {
        public static SerializerState GetSerializableStateFromPCOState(PCOState pCOState)
        {
            SerializerState serialState = new SerializerState();

            serialState.RootFolder = GetSerializerRoot(pCOState.GetExplorerState().GetRoot());
            serialState.Teams = GetSerializerTeams(pCOState.GetContributorState().GetAllTeams());
            serialState.Authors = GetSerializerAuthors(pCOState.GetContributorState().GetAllAuthors());
            serialState.Settings = GetSerializerSettings(pCOState.GetSettingsState());
            serialState.RepositoryRootPath = pCOState.GetExplorerState().GetRootPath();
            serialState.LatestCommitSHA = pCOState.GetLatestCommitSha();

            return serialState;
        }

        private static SerializerSettings GetSerializerSettings(SettingsState settingsState)
        {
            SerializerSettings serialSettings = new SerializerSettings();


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
                serialAuthor.Color= pCOAuthor.Color;

                serialAuthor.SubAuthors = new List<SerializerAuthor>();
                foreach(var pCOSubAuthor in pCOAuthor.SubAuthors)
                {
                    SerializerAuthor serialSubAuthor = new SerializerAuthor();
                    serialSubAuthor.Name = pCOSubAuthor.Name;
                    serialSubAuthor.Email = pCOSubAuthor.Email;
                    serialSubAuthor.Color = pCOSubAuthor.Color;

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

                serialTeams.Add(serialTeam);
            }
            return serialTeams;
        }

        private static SerializerFolder GetSerializerRoot(PCOFolder pCOFolder)
        {
            SerializerFolder serialFolder = new SerializerFolder();
            serialFolder.Name = pCOFolder.Name;

            var selectedOwner = pCOFolder.GraphModel.SelectedOwner;
            if(selectedOwner != null)
            {

                if (selectedOwner.GetType() == typeof(Author))
                {
                    serialFolder.SelectedAuthorEmail = ((Author)selectedOwner).Email;
                    serialFolder.SelectedTeamName = "";
                }
                else
                {
                    serialFolder.SelectedAuthorEmail = "";
                    serialFolder.SelectedTeamName = ((PCOTeam)selectedOwner).Name;
                }
            }

            foreach (var child in pCOFolder.Children.Values)
            {
                if(child.GetType() == typeof(PCOFolder))
                {
                    serialFolder.Children.Add(GetSerializerRoot(child as PCOFolder));
                }
                else
                {
                    serialFolder.Children.Add(GetSerializerFile(child as PCOFile));
                }
            }
            
            return serialFolder;

        }
        private static SerializerFile GetSerializerFile(PCOFile pCOFile)
        {
            SerializerFile serialFile = new SerializerFile();
            serialFile.Name = pCOFile.Name;
            serialFile.CreatorEmail = pCOFile.Creator.Name;
            
            foreach(var commit in pCOFile.commits)
            {
                SerializerCommit serialCommit = new SerializerCommit();
                serialCommit.AuthorEmail = commit.GetAuthor().Email;
                serialCommit.LineCount = commit.GetLines();
                serialCommit.Date = commit.GetDate();
                serialFile.Commits.Add(serialCommit);
            }

            return serialFile;
        }
    }
}

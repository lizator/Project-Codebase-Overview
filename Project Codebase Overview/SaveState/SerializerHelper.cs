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



            return serialState;
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

                serialFile.Commits.Add(serialCommit);
            }

            return serialFile;
        }
    }
}

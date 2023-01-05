using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.Settings;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using static Project_Codebase_Overview.DataCollection.Model.GraphModel;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class PCOFile : ExplorerItem
    {

        public List<PCOCommit> commits;
        public Author Creator { get; set; }

        public PCOFile(string name, PCOFolder parent, List<PCOCommit> commits = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.commits = commits ?? new List<PCOCommit>();
            this.GraphModel = new GraphModel();
            this.GraphModel.FileName = name;
        }

        public override void CalculateData()
        {
            var settingsState = PCOState.GetInstance().GetSettingsState();
            this.GraphModel = new GraphModel();
            this.GraphModel.FileName = this.Name;

            var groupedCommits = this.commits.GroupBy(x => x.GetAuthor());

            foreach (var groupedComm in groupedCommits)
            {
                
                var author = groupedComm.First().GetAuthor();

                foreach (var commit in groupedComm)
                {
                    if (!PCOState.GetInstance().GetSettingsState().IsDateWithinCutOff(commit.GetDate()))
                    {
                        continue;
                    }
                    var linesModified = (uint)settingsState.CalculateLinesAfterDecay(commit.GetLines(), commit.GetDate());
                    
                    if (this.Creator.Equals(author))
                    {
                        linesModified = (uint) Math.Ceiling((double) (((double)settingsState.CreatorBonusPercent / 100) + 1) * linesModified);
                    }

                    this.GraphModel.LinesModified += linesModified;
                    this.GraphModel.LinesTotal += (uint)commit.GetLines();

                    this.GraphModel.LineDistribution.TryAdd(author, new LineDistUnit(0, 0));

                    this.GraphModel.LineDistribution[author].SuggestedLines += linesModified;
                    //this.GraphModel.LineDistribution[author].SuggestedLines += PCOState.GetInstance().GetSettingsState().IsDecayActive ? linesModified : (uint)commit.GetLines();
                }
            }

            if(PCOState.GetInstance().GetSettingsState().CurrentMode == PCOExplorerMode.TEAMS)
            {
                //Mode.TEAMS so convert to teams
                var teamDist = new Dictionary<IOwner, LineDistUnit>();

                foreach(var dist in this.GraphModel.LineDistribution)
                {
                    var teams = new List<IOwner>();
                    if (((Author)dist.Key).Teams.Count != 0)
                    {
                        foreach (var team in ((Author)dist.Key).Teams)
                        {
                            teams.Add(team);
                        }
                    }
                    else
                    {
                        //not in team
                        teams.Add(PCOState.GetInstance().GetContributorState().GetNoTeam());
                    }

                    uint lines = (uint) Math.Ceiling((double) dist.Value.SuggestedLines / teams.Count());
                    //add lines
                    foreach (var team in teams)
                    {
                        
                        if(!teamDist.TryAdd(team, new GraphModel.LineDistUnit(lines, 0))){
                            teamDist[team].SuggestedLines += lines;
                        }
                    }
                }
                this.GraphModel.LineDistribution = teamDist; 
            }


            if (this.GraphModel.LinesTotal > 0)
            {
                this.GraphModel.UpdateSuggestedOwner();
                //this.GenerateBarGraph();
            }

            this.UpdateSelectedOwners();
        }

        public override int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(PCOFile))
            {
                return 1;
            }
            return string.Compare(this.Name, ((PCOFile)obj).Name, StringComparison.InvariantCulture);
        }

        public override string ToCodeowners()
        {
            return this.GetCodeownerLines();
        }
    }
}

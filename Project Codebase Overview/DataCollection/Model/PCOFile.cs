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
                List<IOwner>  owners = new List<IOwner>();//commit can have multiple owners if user is in multiple teams
                if(PCOState.GetInstance().GetSettingsState().CurrentMode == PCOExplorerMode.USER)
                {
                    owners.Add(groupedComm.First().GetAuthor()); 
                }
                else
                {
                    //Mode.TEAMS
                    if(groupedComm.First().GetAuthor().Teams.Count != 0)
                    {
                        foreach(var team in groupedComm.First().GetAuthor().Teams)
                        {
                            owners.Add(team);
                        }
                    }
                    else
                    {
                        //not in team
                        owners.Add(PCOState.GetInstance().GetContributorState().GetNoTeam());
                    }
                }
                foreach (var owner in owners)
                {
                    this.GraphModel.LineDistribution.TryAdd(owner, new GraphModel.LineDistUnit(0,0));
                }

                foreach (var commit in groupedComm)
                {
                    if (!PCOState.GetInstance().GetSettingsState().IsDateWithinCutOff(commit.GetDate()))
                    {
                        continue;
                    }
                    var linesAfterDecay = (uint)settingsState.CalculateLinesAfterDecay(commit.GetLines(), commit.GetDate());
                    this.GraphModel.LinesAfterDecay += linesAfterDecay;
                    this.GraphModel.LinesTotal += (uint)commit.GetLines();
                    foreach(var owner in owners)
                    {
                        this.GraphModel.LineDistribution[owner].SuggestedLines += PCOState.GetInstance().GetSettingsState().IsDecayActive ? linesAfterDecay : (uint)commit.GetLines();
                    }
                }
            }
            if (this.GraphModel.LinesTotal > 0)
            {
                this.GraphModel.UpdateSuggestedOwner();
                this.GenerateBarGraph();
            }
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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class PCOFile : IExplorerItem
    {

        public List<PCOCommit> commits;

        public override void CalculateData()
        {
            //TODO: ask Should this be handled inside graphmodel instead?
            var groupedCommits = this.commits.GroupBy(x => x.GetAuthor());

            foreach (var groupedComm in groupedCommits)
            {
                this.GraphModel.LineDistribution.Add(groupedComm.First().GetAuthor(), 0);
                
                foreach (var commit in groupedComm)
                {
                    this.GraphModel.LinesTotal += (uint) commit.GetLines();
                    this.GraphModel.LineDistribution[commit.GetAuthor()] += (uint) commit.GetLines();  
                }
            }
            this.GraphModel.UpdateSuggestedOwner();
        }

        public override int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(PCOFile))
            {
                return 1;
            }
            return string.Compare(this.Name, ((PCOFile)obj).Name, StringComparison.InvariantCulture);
        }

        public string SuggestedOwnerName { get => this.GraphModel.SuggestedOwner?.Name; }

        public PCOFile(string name, PCOFolder parent, List<PCOCommit> commits = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.commits = commits ?? new List<PCOCommit>();
            this.GraphModel = new GraphModel();
            this.GraphModel.FileName = name;
        }

    }
}

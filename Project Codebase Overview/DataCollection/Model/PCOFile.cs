using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
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
        public string LinesTotal { get => this.GraphModel.LinesTotal.ToString("N0", CultureInfo.InvariantCulture); }
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
            var groupedCommits = this.commits.GroupBy(x => x.GetAuthor());

            foreach (var groupedComm in groupedCommits)
            {
                this.GraphModel.LineDistribution.Add(groupedComm.First().GetAuthor(), 0);

                foreach (var commit in groupedComm)
                {
                    this.GraphModel.LinesTotal += (uint)commit.GetLines();
                    this.GraphModel.LineDistribution[commit.GetAuthor()] += (uint)commit.GetLines();
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

    }
}

using Project_Codebase_Overview.ContributorManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class GraphModel
    {
        public IOwner SuggestedOwner;
        public IOwner SelectedOwner;
       
        public Dictionary<IOwner, uint> LineDistribution;
        public uint LinesAfterDecay;
        public uint LinesTotal;
        public string FileName;

        public GraphModel()
        {
            this.LineDistribution = new Dictionary<IOwner, uint>();
            this.LinesAfterDecay = 0;
            this.LinesTotal = 0;
            SuggestedOwner = null;
            SelectedOwner = null;
        }

        public void AddLineDistributions(Dictionary<IOwner, uint> childLineDistributions)
        {
            foreach(var childLineDistribution in childLineDistributions)
            {
                if (this.LineDistribution.ContainsKey(childLineDistribution.Key))
                {
                    this.LineDistribution[childLineDistribution.Key] += childLineDistribution.Value;
                }
                else
                {
                    this.LineDistribution.Add(childLineDistribution.Key, childLineDistribution.Value);
                }
            }
        }

        internal void UpdateSuggestedOwner()
        {
            var highestValue = this.LineDistribution.Values.Max();
            this.SuggestedOwner = this.LineDistribution.First(x => x.Value == highestValue).Key;
        }
    }
}

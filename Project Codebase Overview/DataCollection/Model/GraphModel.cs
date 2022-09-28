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
        IOwner SelectedOwner;
       
        public Dictionary<Author, uint> LineDistribution;
        public uint LinesTotal;
        public string FileName;

        public GraphModel()
        {
            this.LineDistribution = new Dictionary<Author, uint>();
            this.LinesTotal = 0;
            SuggestedOwner = null;
            SelectedOwner = null;
        }

        public void AddLineDistributions(Dictionary<Author, uint> childLineDistributions)
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

        //For UI purposes
        public Dictionary<Author, uint> GetAuthorDistribution()
        {

            throw new NotImplementedException();
        }
        //For UI purposes
        public Dictionary<IOwner, double> GetTeamDistribution()
        {
            //Count line distribution according to team setup
            
            throw new NotImplementedException();
        }

        internal void UpdateSuggestedOwner()
        {
            var highestValue = this.LineDistribution.Values.Max();
            this.SuggestedOwner = this.LineDistribution.First(x => x.Value == highestValue).Key;
        }
    }
}

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
        IOwner SuggestedOwner;
        IOwner SelectedOwner;
       
        public Dictionary<Author, uint> LineDistribution;
        uint LinesTotal;

        public GraphModel()
        {
            this.LineDistribution = new Dictionary<Author, uint>();
            this.LinesTotal = 0;
        }

        public void AddLineDistribution(Dictionary<Author, uint> childLineDistributions)
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
    }
}

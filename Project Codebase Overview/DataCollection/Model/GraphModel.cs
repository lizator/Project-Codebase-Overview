using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public class GraphModel
    {
        public class LineDistUnit
        {
            public double? TempModifiedLines; //This is used to collect an authors entire commit lines after decay and creator bonus before rounding it to a int in suggested lines (only used in PCOFile)
            public uint SuggestedLines;
            public uint SelectedLines;
            public LineDistUnit(uint suggestedLines, uint selectedLines)
            {
                SuggestedLines = suggestedLines;
                SelectedLines = selectedLines;
            }
            public uint LineSum()
            {
                return SuggestedLines + SelectedLines;
            }
        }
        
        public IOwner SuggestedOwner;
        public Dictionary<IOwner, LineDistUnit> LineDistribution;
        public uint LinesModified;
        public uint LinesTotal;
        public string FileName;

        public GraphModel()
        {
            this.LineDistribution = new Dictionary<IOwner, LineDistUnit>();
            this.LinesModified = 0;
            this.LinesTotal = 0;
            SuggestedOwner = null;
        }

        public void AddLineDistributions(Dictionary<IOwner, LineDistUnit> childLineDistributions)
        {
            foreach(var childLineDistribution in childLineDistributions)
            {
                if (this.LineDistribution.ContainsKey(childLineDistribution.Key))
                {
                    this.LineDistribution[childLineDistribution.Key].SelectedLines += childLineDistribution.Value.SelectedLines;
                    this.LineDistribution[childLineDistribution.Key].SuggestedLines += childLineDistribution.Value.SuggestedLines;
                }
                else
                {
                    this.LineDistribution.Add(childLineDistribution.Key, new LineDistUnit(childLineDistribution.Value.SuggestedLines, childLineDistribution.Value.SelectedLines));
                }
            }
        }
        public void AddLineDistributions_SelectedOwner(Dictionary<IOwner, LineDistUnit> childLineDistributions, List<IOwner> selectedOwners)
        {
            var convertedOwners = new List<IOwner>();
            //convert selected owners to current mode (teams or authors)
            if (PCOState.GetInstance().GetSettingsState().CurrentMode == Settings.PCOExplorerMode.AUTHOR)
            {
                //author mode
                foreach (var selectedOwner in selectedOwners)
                {
                    if (selectedOwner.GetType() == typeof(Author))
                    {
                        convertedOwners.Add(selectedOwner);
                    }
                    else
                    {
                        foreach (var author in ((PCOTeam)selectedOwner).Members)
                        {
                            if (!convertedOwners.Contains(author))
                            {
                                convertedOwners.Add(author);
                            }
                        }
                    }
                }
            }
            else
            {
                //Team mode
                foreach(var selectedOwner in selectedOwners)
                {
                    if(selectedOwner.GetType() == typeof(Author))
                    {
                        //handle author to team convert
                        foreach(var team in ((Author)selectedOwner).Teams)
                        {
                            if (!convertedOwners.Contains(team))
                            {
                                convertedOwners.Add(team);
                            }
                        }
                    }
                    else
                    {
                        //handle team
                        if (!convertedOwners.Contains(selectedOwner))
                        {
                            convertedOwners.Add(selectedOwner);
                        }
                    }
                }
            }
            


            foreach (var childLineDistribution in childLineDistributions)
            {
                //add child suggested lines to selectedowner's selectedLines
                foreach(var convertedOwner in convertedOwners)
                {
                    if (this.LineDistribution.ContainsKey(convertedOwner))
                    {
                        this.LineDistribution[convertedOwner].SelectedLines += (uint)(childLineDistribution.Value.SuggestedLines / convertedOwners.Count);
                    }
                    else
                    {
                        this.LineDistribution.Add(convertedOwner, new LineDistUnit(0, (uint)(childLineDistribution.Value.SuggestedLines / convertedOwners.Count)));
                    }
                }

                //add child selected lines to their respective selected owners (ignore suggested, theyre handled above)
                if (this.LineDistribution.ContainsKey(childLineDistribution.Key))
                {
                    this.LineDistribution[childLineDistribution.Key].SelectedLines += childLineDistribution.Value.SelectedLines;
                }
                else
                {
                    this.LineDistribution.Add(childLineDistribution.Key, new LineDistUnit(0, childLineDistribution.Value.SelectedLines));
                }
            }
        }

        internal void UpdateSuggestedOwner()
        {
            var activeLineDistributions = this.LineDistribution.Where(x => x.Key.IsActive);
            this.SuggestedOwner = activeLineDistributions.Count() > 0 ? activeLineDistributions.OrderByDescending(x => x.Value.LineSum()).First().Key : null;
        }
    }
}

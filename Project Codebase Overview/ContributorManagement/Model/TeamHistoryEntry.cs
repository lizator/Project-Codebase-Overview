using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    public class TeamHistoryEntry
    {
        public string Team { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset? To { get; set; }

        public TeamHistoryEntry(string team, DateTimeOffset from, DateTimeOffset? to)
        {
            Team = team;
            From = from;
            To = to;
        }
    }
}

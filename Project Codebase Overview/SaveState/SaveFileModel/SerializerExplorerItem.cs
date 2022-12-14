using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState.Model
{
    public abstract class SerializerExplorerItem
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<string> SelectedAuthorEmails { get; set; }
        public List<string> SelectedTeamNames { get; set; }
        public string Comment { get; set; }

    }
}

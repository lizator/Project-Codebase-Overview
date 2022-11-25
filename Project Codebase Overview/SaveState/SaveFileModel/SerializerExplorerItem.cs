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
        public string SelectedAuthorEmail { get; set; }
        public string SelectedTeamName { get; set; }

    }
}

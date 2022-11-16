using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerState
    {
        public List<SerializerAuthor> Authors { get; set; }
        public List<SerializerTeam> Teams { get; set; }
        public SerializerExplorerItem RootFolder { get; set; }
        public SerializerSettings Settings { get; set; }

        public SerializerState()
        {
            Authors = new List<SerializerAuthor>();
            Teams = new List<SerializerTeam>();
        }
    }
}

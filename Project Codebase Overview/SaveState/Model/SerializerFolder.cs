using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerFolder : SerializerExplorerItem
    {
        public List<SerializerExplorerItem> Children { get; set; }
        public SerializerFolder()
        {
            Children = new List<SerializerExplorerItem>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerFolder : SerializerExplorerItem
    {
        public List<SerializerFile> SubFiles { get; set; }

        public List<SerializerFolder> SubFolders { get; set; }  
        public SerializerFolder()
        {
            SubFiles = new List<SerializerFile>();
            SubFolders = new List<SerializerFolder>();
        }
    }
}

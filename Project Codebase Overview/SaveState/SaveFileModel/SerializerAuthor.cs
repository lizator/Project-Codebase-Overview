using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerAuthor
    {
        public string Name { get; set; }
        public List<string> Aliases {get; set; }
        public Color Color { get; set; }
        public string Email { get; set; }
        public string VCSEmail { get; set; }
        public List<SerializerAuthor> SubAuthors { get; set; }
        public bool IsActive { get; set; }
        
        public SerializerAuthor()
        {

        }
    }
}

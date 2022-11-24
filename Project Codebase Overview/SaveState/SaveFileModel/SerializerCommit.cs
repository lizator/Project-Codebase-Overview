using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerCommit
    {
        public string AuthorEmail { get; set; }
        public int LineCount { get; set; }

        public DateTime Date { get; set;}
    }
}

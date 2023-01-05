using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerTeam
    {
        public string Name { get; set; }
        public List<string> MemberEmails { get; set; }
        public Color Color { get; set; }
        public string VCSID { get; set; }
    }
}

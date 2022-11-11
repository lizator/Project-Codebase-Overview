using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    public interface IOwner
    {
        string Name { get; set; }
        Color Color { get; set; }
        public bool ContainsEmail(string email);
        
    }
}

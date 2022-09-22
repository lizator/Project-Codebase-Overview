using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.ContributorManagement.Model
{
    public interface IOwner
    {
        string Name { get; set; }
        string Color { get; set; }
        
    }
}

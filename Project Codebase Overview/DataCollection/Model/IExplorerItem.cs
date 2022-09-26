using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public interface IExplorerItem : IComparable
    {
        public string Name { get; set; }
        public void CalculateData();
        public GraphModel GraphModel { get;  }
        public PCOFolder Parent { get; set; }
        public string SuggestedOwnerName { get; }

    }
}

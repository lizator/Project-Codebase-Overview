using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    internal interface IExplorerItem
    {
        public string name { get; set; }
        public void CalculateData();
        public object graphModel { get;  }
        public Folder parent { get; set; }

    }
}

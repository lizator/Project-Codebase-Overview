using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection
{
    internal interface IVCSDataCollector
    {
        public object CollectAllData(string path);

    }
}

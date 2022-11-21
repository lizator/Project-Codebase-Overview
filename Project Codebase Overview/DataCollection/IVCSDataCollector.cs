using Project_Codebase_Overview.DataCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection
{
    internal interface IVCSDataCollector
    {
        public Task<PCOFolder> CollectAllData(string path);

        public Task<PCOFolder> CollectNewData(string path, PCOFolder oldDataRootFolder, string lastLoadedCommitSHA);
    }
}

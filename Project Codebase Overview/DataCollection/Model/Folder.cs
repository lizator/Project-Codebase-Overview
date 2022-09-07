using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    internal class Folder : IExplorerItem
    {
        public string name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object graphModel => throw new NotImplementedException();

        public IExplorerItem parent => throw new NotImplementedException();

        public void CalculateData()
        {
            throw new NotImplementedException();
        }

        public List<IExplorerItem> children => throw new NotImplementedException();
    }
}

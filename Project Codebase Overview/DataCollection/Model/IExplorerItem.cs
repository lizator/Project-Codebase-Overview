﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public interface IExplorerItem : IComparable
    {
        public string name { get; set; }
        public void CalculateData();
        public object graphModel { get;  }
        public PCOFolder parent { get; set; }

    }
}

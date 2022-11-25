﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerFile : SerializerExplorerItem
    {
        public string CreatorEmail { get; set; }
        public List<SerializerCommit> Commits { get; set; }

        public SerializerFile()
        {
            Commits = new List<SerializerCommit>();
        }
    }
}

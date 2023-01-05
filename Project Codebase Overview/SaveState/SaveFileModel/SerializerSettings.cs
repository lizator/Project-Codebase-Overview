using Project_Codebase_Overview.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.SaveState.Model
{
    public class SerializerSettings
    {
        PCOExplorerMode CurrentMode { get; set; }
        public bool IsDecayActive {get; set; }
        public int DecayDropOffInteval { get; set; }
        public int DecayPercentage { get; set; }
        public DecayTimeUnit DecayTimeUnit { get; set; }
        public int CreatorMultiplier { get; set; }
    }
}

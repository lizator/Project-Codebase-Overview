using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.State
{
    public enum Mode
    {
        USER,
        TEAMS
    }
    public class SettingsState
    {
        
        public Mode currentMode;

        
    }
}

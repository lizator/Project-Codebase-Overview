using Project_Codebase_Overview.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.State
{
    
    public class SettingsState
    {
        
        public PCOExplorerMode CurrentMode;
        public bool IsDecayActive;
        
        public int DecayDropOffInteval;
        public int DecayPercentage;
        public DecayTimeUnit DecayTimeUnit;

        public SettingsState()
        {
            CurrentMode = PCOExplorerMode.USER;
            IsDecayActive = false;
            DecayDropOffInteval = 0;
            DecayPercentage = 0;
            DecayTimeUnit = DecayTimeUnit.UNDEFINED;
        }

        public double CalculateLinesAfterDecay(int lineCount, DateTime commitDate)
        {
            if (!IsDecayActive || DecayDropOffInteval == 0 || DecayPercentage == 0)
            {
                //If decay is not active or settings not set correctly, just return lines total
                return lineCount;
            }

            var ticks = CalculateTicks(commitDate);

            return lineCount * Math.Pow(1- ((float)DecayPercentage / 100), ticks);
        }

        private int CalculateTicks(DateTime commitDate)
        {
            if(DecayDropOffInteval <= 0)
            {
                throw new Exception("DecayDropOffInterval cannot be 0 or less");
            }

            var currDate = DateTime.Now;

            var diff = (int)(currDate - commitDate).TotalDays;

            var TickTimeInDays = 1;
            switch(DecayTimeUnit)
            {
                case DecayTimeUnit.DAY:
                    TickTimeInDays = DecayDropOffInteval;
                    break;
                case DecayTimeUnit.WEEK:
                    TickTimeInDays = DecayDropOffInteval * 7;
                    break;
                case DecayTimeUnit.MONTH:
                    TickTimeInDays = DecayDropOffInteval * 30;
                    break;

                case DecayTimeUnit.YEAR:
                    TickTimeInDays = DecayDropOffInteval * 365;
                    break;

                default:
                    TickTimeInDays = diff;
                    break;
            }

            return (int)(diff/TickTimeInDays);
        }
    }
}

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
        public bool IsFilesVisibile;
        
        public int DecayDropOffInteval;
        public int DecayPercentage;
        public DecayTimeUnit DecayTimeUnit;

        public CutOffSelectionUnit CutOffSelectionUnit;

        public SettingsState()
        {
            CurrentMode = PCOExplorerMode.USER;
            IsDecayActive = false;
            IsFilesVisibile = true;
            DecayDropOffInteval = 1;
            DecayPercentage = 25;
            DecayTimeUnit = DecayTimeUnit.MONTH;
            CutOffSelectionUnit = CutOffSelectionUnit.ALL_TIME;
        }

        public bool IsDateWithinCutOff(DateTime commitDate)
        {
            if (CutOffSelectionUnit == CutOffSelectionUnit.ALL_TIME)
            {
                return true;
            }

            var currDate = DateTime.Now;

            var diff = (int)(currDate - commitDate).TotalDays;

            int cutOffInDays;
            switch (CutOffSelectionUnit)
            {
                case CutOffSelectionUnit.SIX_MONTHS:
                    cutOffInDays = 6*30;
                    break;
                case CutOffSelectionUnit.ONE_YEAR:
                    cutOffInDays = 365;
                    break;
                case CutOffSelectionUnit.TWO_YEARS:
                    cutOffInDays = 2*365;
                    break;
                case CutOffSelectionUnit.THREE_YEARS:
                    cutOffInDays = 3*365;
                    break;
                case CutOffSelectionUnit.FIVE_YEARS:
                    cutOffInDays = 5*365;
                    break;
                default:
                    throw new Exception("CutOffSelection not chosen");
            }

            return diff <= cutOffInDays;
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

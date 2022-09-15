using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.Settings.Model
{
    internal class Decay
    {
        public bool isActive = false;
        public int timeCounter = 0;
        public DecayTimeUnit timeUnit = DecayTimeUnit.UNDÈFINED;
        public int dropOffPercentage = 0;

        public double CalculateLinesAfterDecay(int lineCount, DateTime commitDate)
        {
            var ticks = CalculateTicks(commitDate);

            return lineCount * Math.Pow((float)dropOffPercentage / 100, ticks);
        }

        private int CalculateTicks(DateTime commitDate)
        {
            throw new NotImplementedException();
        }
    }

    internal enum DecayTimeUnit
    {
        UNDÈFINED = 0,
        DAY = 1,
        WEEK = 2,
        MONTH = 3,
        YEAR = 4,
    }
}

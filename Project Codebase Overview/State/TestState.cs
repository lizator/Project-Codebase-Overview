using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.State
{
    public class TestState
    {

        private static readonly string[] _WatchNames = { "Blame" };

        private Dictionary<string, Stopwatch> TestWatches;
        private Dictionary<string, int> WatchUseCount;
        private Dictionary<string, long> WatchTimesTotalMS;

        public TestState()
        {
            this.TestWatches = new Dictionary<string, Stopwatch>();
            this.WatchUseCount = new Dictionary<string, int>();
            this.WatchTimesTotalMS = new Dictionary<string, long>();
            foreach(var watchName in _WatchNames)
            {
                this.TestWatches.Add(watchName, new Stopwatch());
                this.WatchUseCount.Add(watchName, 0);
                this.WatchTimesTotalMS.Add(watchName, 0);
            }

        }

        public void StartWatch(string name)
        {
            this.TestWatches[name].Start();
        }

        public void StopWatch(string name)
        {
            this.TestWatches[name].Stop();

            this.WatchUseCount[name] += 1;
            this.WatchTimesTotalMS[name] += this.TestWatches[name].ElapsedMilliseconds;

            this.TestWatches[name].Reset();

        }

        public void AddWatchTime(string name, long timeMS)
        {
            this.WatchUseCount[name] += 1;
            this.WatchTimesTotalMS[name] += timeMS;
        }

        public void PrintWatches()
        {
            foreach (var watchName in _WatchNames)
            {
                Debug.WriteLine(String.Format("name: \"{0}\", Times used: {1}, Total time: {2}ms or {3}min, Average time: {4}ms", 
                    watchName,
                    this.WatchUseCount[watchName],
                    this.WatchTimesTotalMS[watchName],
                    this.WatchTimesTotalMS[watchName] / 60000,
                    this.WatchTimesTotalMS[watchName] / this.WatchUseCount[watchName]));
            }
        }



    }
}

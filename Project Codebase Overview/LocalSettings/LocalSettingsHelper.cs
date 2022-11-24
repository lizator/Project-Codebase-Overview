using Newtonsoft.Json;
using Syncfusion.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Project_Codebase_Overview.LocalSettings
{
    public class LocalSettingsHelper
    {
        readonly static string TAG_RECENT_LIST = "recent";

        public static void AddRecentFile(string fileName, string repoName, string filePath)
        {
            List<RecentFileInfo> recentList = GetRecentFiles();

            //get index of filepath to add (if its there, else returns -1)
            var index = recentList.Select(x => x.FilePath).ToList().IndexOf(filePath);
            if (index != -1)
            {
                //the file is already on the list - move it up
                recentList.MoveTo(index, 0);
            }
            else
            {
                if (recentList.Count == 5)
                {
                    recentList.Remove(recentList.Last());
                }
                recentList.Add(new RecentFileInfo(fileName, repoName, filePath));
            }

            //save recent list
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[TAG_RECENT_LIST] = JsonConvert.SerializeObject(recentList);
        }
        
        public static List<RecentFileInfo> GetRecentFiles()
        {
            //Load recent list
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string jsonRecentList = localSettings.Values[TAG_RECENT_LIST] as string;
            List<RecentFileInfo> recentList;
            if (jsonRecentList == null)
            {
                recentList = new List<RecentFileInfo>();
            }
            else
            {
                recentList = JsonConvert.DeserializeObject<List<RecentFileInfo>>(jsonRecentList);
            }
            return recentList;
        }

    }
}

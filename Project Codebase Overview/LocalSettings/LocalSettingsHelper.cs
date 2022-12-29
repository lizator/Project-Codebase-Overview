using LibGit2Sharp;
using Newtonsoft.Json;
using Project_Codebase_Overview.State;
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
        readonly static string TAG_EXPLORER_UPDATE_EXPLAINED = "explorer_explained";

        public static void AddRecentFile(string fileName, string repoName, string filePath)
        {
            List<RecentFileInfo> recentList = GetRecentFiles();

            //get index of filepath to add (if its there, else returns -1)
            var index = recentList.Select(x => x.FilePath).ToList().IndexOf(filePath);
            if (index != -1)
            {
                //the file is already on the list - move it up and update date
                recentList[index].DateString = DateTime.Now.Date.ToShortDateString();
                recentList.MoveTo(index, 0);
            }
            else
            {
                if (recentList.Count == 5)
                {
                    recentList.Remove(recentList.Last());
                }
                recentList.Insert(0, new RecentFileInfo(fileName, repoName, filePath, DateTime.Now.Date.ToShortDateString()));
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

        public static void SetIsExplorerUpdateExplained(bool value = true)
        {
            PCOState.GetInstance().IsExplorerUpdateExplained = value;
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[TAG_EXPLORER_UPDATE_EXPLAINED] = value;
        }

        public static bool GetIsExplorerUpdateExplained()
        {
            if (!PCOState.GetInstance().IsExplorerUpdateExplained)
            {
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                PCOState.GetInstance().IsExplorerUpdateExplained = ((bool?)localSettings.Values[TAG_EXPLORER_UPDATE_EXPLAINED]) ?? false;
            }
            return PCOState.GetInstance().IsExplorerUpdateExplained;
        }

    }
}

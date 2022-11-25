using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.LocalSettings
{
    public class RecentFileInfo
    {
        public string FileName { get; set; }
        public string RepoName { get; set; }
        public string FilePath { get; set; }
        public string DateString { get; set; }
        
        public RecentFileInfo(string fileName, string repoName, string filePath, string dateString)
        {
            FileName = fileName;
            RepoName = repoName;
            FilePath = filePath;
            DateString = dateString;
        }
    }
}

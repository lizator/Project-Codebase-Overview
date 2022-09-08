using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Project_Codebase_Overview.DataCollection.Model;
using Windows.System;

namespace Project_Codebase_Overview.DataCollection
{
    internal class GitDataCollector : IVCSDataCollector
    {
        public object CollectAllData(string path)
        {
            var rootPath = "C:\\Users\\Jacob\\source\\repos\\lizator\\Project-Codebase-Overview";
           
            Folder rootFolder = new Folder("root", null);
            
            var repo = new Repository(rootPath);

            int cutRoot = rootPath.Length + 1; //index to remove rootpath + \\ from paths
            var blameOptions = new BlameOptions();


            string[] topFolders = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly)
                .Select(file => file.Substring(cutRoot))
                .Where(file => !file.StartsWith(".")).ToArray();

            foreach (string topFolder in topFolders)
            {
                rootFolder.children.Add(new Folder(topFolder, rootFolder));

                // TODO MAKE RECURSIVE ALGORITHM USING SEARCHOPTIONS.TOPDIRECTORIES STARTING FROM ROOT INSTEAD!

                foreach (string file in Directory.EnumerateFiles(rootPath, topFolder + "/*.*", SearchOption.AllDirectories))
                {
                    // for all filepaths within topfolders
                    var filePathClean = file.Substring(cutRoot).Replace("\\", "/");
                    var fileBlameHunkCollection = repo.Blame(filePathClean);

                    var groups = fileBlameHunkCollection.GroupBy(hunk => hunk.FinalCommit.Id);

                    foreach (var group in groups)
                    {

                    }

                    Dictionary<string, BlameHunk[]> blameHunkGroups = new Dictionary<string, BlameHunk[]>();
                    
                    foreach (BlameHunk blameHunk in fileBlameHunkCollection)
                    {
                      
                    }

                }
            }
            

           
            var blame = repo.Blame("Project Codebase Overview/TestDocs/TextFile1.txt", blameOptions);
            System.Diagnostics.Debug.WriteLine(blame.ToString());
            return null;
        }
    }
}

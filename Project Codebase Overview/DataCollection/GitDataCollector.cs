using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Project_Codebase_Overview.DataCollection.Model;
using Windows.System;
using static System.Net.WebRequestMethods;

namespace Project_Codebase_Overview.DataCollection
{
    internal class GitDataCollector : IVCSDataCollector
    {
        string gitRoot;
        int cutRootPath;
        Repository gitRepo;
        Folder gitRootFolder;

        public object CollectAllData(string path)
        {
            //after stuff is below
            // gitRoot = path;
            gitRoot = "C:\\Users\\Jacob\\source\\repos\\lizator\\Project-Codebase-Overview";
            cutRootPath = gitRoot.Length + 1;
            gitRepo = new Repository(gitRoot);

            gitRootFolder = recurseTree(gitRoot, null);

            int winner = 1;
           /*
            //before stuff is below 
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
                rootFolder.addChild(new Folder(topFolder, rootFolder));

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
           */
            return null;
        }

        private Folder recurseTree(string currentPath, Folder parent)
        {
            //create current folder
            var currentFolder = new Folder(currentPath, parent);

            int currentPathCut = currentPath.Length + 1;

            //get all subfolderpaths
            var subFolderPaths = Directory.EnumerateDirectories(currentPath, "*", SearchOption.TopDirectoryOnly)
                .Where(file => !file.Substring(currentPathCut).StartsWith(".")
                            && !file.Substring(currentPathCut).StartsWith("bin")
                            && !file.Substring(currentPathCut).StartsWith("obj"))
                .ToArray(); 

            foreach (var subfolder in subFolderPaths)
            { 
                //recurese children
                var childFolder = recurseTree(subfolder, currentFolder);
                currentFolder.addChild(childFolder);
            }
            
            // get file data in current folder
            var filePaths = Directory.EnumerateFiles(currentPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in filePaths)
            {
                var filePathClean = filePath.Substring(cutRootPath).Replace("\\", "/");
                var fileBlameHunkCollection = gitRepo.Blame(filePathClean);
                var groups = fileBlameHunkCollection.GroupBy(hunk => hunk.FinalCommit.Id);

                PCOFile file = new PCOFile(filePathClean, currentFolder);
                currentFolder.addChild(file);

                foreach (var group in groups)
                {
                    int commitLineCount = group.Sum(hunk => hunk.LineCount);
                    var finalSignature = group.First().FinalSignature;
                    file.commits.Add(new PCOCommit(commitLineCount, 0, 0, finalSignature.Email, finalSignature.Name, finalSignature.When.Date));
                }
            }

            return currentFolder;
        }
    }
}

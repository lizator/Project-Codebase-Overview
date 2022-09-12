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
        string rootPath;
        int cutRootPath;
        Repository gitRepo;
        PCOFolder rootFolder;

        public PCOFolder CollectAllData(string path)
        {
            //after stuff is below
            // gitRoot = path;
            rootPath = "C:\\Users\\Jacob\\source\\repos\\lizator\\Project-Codebase-Overview";
            cutRootPath = rootPath.Length + 1;
            gitRepo = new Repository(rootPath);


            string rootParentPath = Directory.GetParent(rootPath).FullName;
            PCOFolder rootParent = new PCOFolder(rootParentPath, null); // is only used for folder name integrity

            rootFolder = RecurseTree(rootPath, rootParent);

            rootFolder.parent = null; //detach rootFolder from parent
           
            return rootFolder;
        }

        private PCOFolder GetData()
        {
            //gitRepo.RetrieveStatus


            return rootFolder;
        }


        private PCOFolder RecurseTree(string currentPath, PCOFolder parent)
        {
            //create current folder
            var currentFolder = new PCOFolder(currentPath, parent);

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
                var childFolder = RecurseTree(subfolder, currentFolder);
                currentFolder.addChild(childFolder);
            }
            
            // get file data in current folder
            var filePaths = Directory.EnumerateFiles(currentPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in filePaths)
            {
                var filePathClean = filePath.Substring(cutRootPath).Replace("\\", "/");
                var fileBlameHunkCollection = gitRepo.Blame(filePathClean);
                var groups = fileBlameHunkCollection.GroupBy(hunk => hunk.FinalCommit.Id);

                PCOFile file = new PCOFile(filePath.Substring(currentPathCut), currentFolder);
                currentFolder.addChild(file);

                foreach (var group in groups)
                {
                    int commitLineCount = group.Sum(hunk => hunk.LineCount);
                    var finalSignature = group.First().FinalSignature;
                    file.commits.Add(new PCOCommit(commitLineCount, 0, 0, finalSignature.Email, finalSignature.Name, finalSignature.When.Date));
                }
            }

            //set names of folder before return
            // parent name is parent path so current name is currentpath - parentpath
            currentFolder.name = currentFolder.name.Substring(parent.name.Length + 1);

            return currentFolder;
        }
    }
}

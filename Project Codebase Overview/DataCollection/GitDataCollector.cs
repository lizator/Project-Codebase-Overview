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

            var foldergang = GetData();
            
            
            return rootFolder;
        }


        private PCOFolder GetData()
        {
            RepositoryStatus gitStatus = gitRepo.RetrieveStatus(new StatusOptions() { IncludeUnaltered = true });

            //check if there are altered files (IS NOT ALLOWED)
            if (gitStatus.IsDirty)
            {
                //throw new Exception("Repository contains dirty files. Commit all changes and retry.");
            }

            List<string> filePaths = gitStatus.Select(statusEntry => statusEntry.FilePath).Where(x => !x.EndsWith("/")).ToList();
            
            //create root folder
            var rootFolderName = Path.GetFileName(rootPath);
            rootFolder = new PCOFolder(rootFolderName, null);
           
            foreach (string filePath in filePaths)
            {
                PCOFile addedFile = rootFolder.AddChildRecursive(filePath.Split("/"), 0);
                AddFileCommits(addedFile, filePath);
            }

            return rootFolder;
        }

        private void AddFileCommits(PCOFile file, string filePath)
        {
            var blameHunkGroups = gitRepo.Blame(filePath).GroupBy(hunk => hunk.FinalCommit.Id);

            foreach (var group in blameHunkGroups)
            {
                int commitLineCount = group.Sum(hunk => hunk.LineCount);
                var finalSignature = group.First().FinalSignature;
                file.commits.Add(new PCOCommit(commitLineCount, 0, 0, finalSignature.Email, finalSignature.Name, finalSignature.When.Date));
            }
        }
    }
}

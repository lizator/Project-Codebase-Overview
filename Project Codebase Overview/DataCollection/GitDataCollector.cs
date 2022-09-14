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

        Repository gitRepo;
        public PCOFolder CollectAllData(string path)
        {
            var rootPath = path;
            //var rootPath = "C:\\Users\\Jacob\\source\\repos\\lizator\\Project-Codebase-Overview";
            //var rootPath = "C:\\Users\\frede\\source\\repos\\Project Codebase Overview";
            try
            {
                gitRepo = new Repository(rootPath);
            }
            catch (Exception e)
            { 
                throw new Exception("The selected directory does not contain a git repository.");
            }
         

            RepositoryStatus gitStatus = gitRepo.RetrieveStatus(new StatusOptions() { IncludeUnaltered = true });

            //check if there are altered files (IS NOT ALLOWED)
            if (gitStatus.IsDirty)
            {
                throw new Exception("Repository contains dirty files. Commit all changes and retry.");
            }

            List<string> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath).ToList();
            
            //create root folder
            var rootFolderName = Path.GetFileName(rootPath);
            var rootFolder = new PCOFolder(rootFolderName, null);
           
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

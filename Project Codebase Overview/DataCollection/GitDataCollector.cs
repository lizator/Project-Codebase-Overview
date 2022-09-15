using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.State;
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
            //rootPath = "C:\\Users\\Jacob\\source\\repos\\lizator\\Project-Codebase-Overview";
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
                //throw new Exception("Repository contains dirty files. Commit all changes and retry.");
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
            var blameHunkGroups = gitRepo.Blame(filePath).GroupBy(hunk => hunk.FinalCommit.Sha);

            foreach (var group in blameHunkGroups)
            {
                int commitLineCount = group.Sum(hunk => hunk.LineCount);
                var finalSignature = group.First().FinalSignature;
                file.commits.Add(new PCOCommit(commitLineCount, 0, 0, finalSignature.Email, finalSignature.Name, finalSignature.When.Date));
            }
        }


        public PCOFolder AlternativeCollectAllData(string path)
        {
            var rootPath = path;
            try
            {
                PCOState.GetInstance().TempGitRepo = new Repository(rootPath);
            }
            catch (Exception e)
            {
                throw new Exception("The selected directory does not contain a git repository.");
            }


            RepositoryStatus gitStatus = PCOState.GetInstance().TempGitRepo.RetrieveStatus(new StatusOptions() { IncludeUnaltered = true });

            //check if there are altered files (IS NOT ALLOWED)
            if (gitStatus.IsDirty)
            {
                throw new Exception("Repository contains dirty files. Commit all changes and retry.");
            }


            //create root folder
            var rootFolderName = Path.GetFileName(rootPath);
            var rootFolder = new PCOFolder(rootFolderName, null);

            List<string[]> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath.Split("/")).ToList();

            rootFolder.AddChildrenAlternativly(filePaths);

            return rootFolder;
        }
        public static void AddFileCommitsAlt(PCOFile file, string filePath)
        {
            var blameHunkGroups = PCOState.GetInstance().TempGitRepo.Blame(filePath).GroupBy(hunk => hunk.FinalCommit.Id);

            foreach (var group in blameHunkGroups)
            {
                int commitLineCount = group.Sum(hunk => hunk.LineCount);
                var finalSignature = group.First().FinalSignature;
                file.commits.Add(new PCOCommit(commitLineCount, 0, 0, finalSignature.Email, finalSignature.Name, finalSignature.When.Date));
            }
        }

        public void testTime()
        {
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            var repetitions = 50;
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            for (int i = 0; i < repetitions; i++)
            {
                //CollectAllData(path);
                AlternativeCollectAllData(path);
                if (i%10 == 0)
                {
                    Debug.WriteLine(i);
                }
            }
            stopwatch.Stop();

            Debug.WriteLine("Average elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds/repetitions);
            
            /*
            ALL PROGRAMS CLOSED
            Results:
            -Version 1, average time 1746
            -Using sha instead of id for groouping of commits, time 1723
            -version 2(creating breadth first ish setup) time 1768

            */
        
        }
    }
}

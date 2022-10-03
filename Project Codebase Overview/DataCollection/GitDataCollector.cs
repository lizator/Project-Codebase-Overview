using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.State;
using Windows.System;
using static System.Net.WebRequestMethods;

namespace Project_Codebase_Overview.DataCollection
{
    internal class GitDataCollector : IVCSDataCollector
    {

        Repository GitRepo;
        string RootPath;
        private readonly object RootFolderLock = new object();


        private static readonly Regex GIT_BLAME_REGEX = new Regex(@"([a-f0-9]+) .*\(<(.+)>[ ]+([0-9]{4}-[0-9]{2}-[0-9]{2}) [0-9]{2}:[0-9]{2}:[0-9]{2} [-\+]{0,1}[0-9}{4}[ ]+[0-9]\)(.*)");
        private static readonly Regex AUTHOR_REGEX = new Regex(@"Author: (.+) <(.+)>");

        public PCOFolder CollectAllData(string path)
        {
            //return this.SimpleCollectAllData(path);
            return this.ParallelGetAllData(path);
            
        }

        public PCOFolder SimpleCollectAllData(string path)
        {
            RootPath = path;
            //rootPath = "C:\\Users\\Jacob\\source\\repos\\lizator\\Project-Codebase-Overview";
            //var rootPath = "C:\\Users\\frede\\source\\repos\\Project Codebase Overview";
            try
            {
                GitRepo = new Repository(RootPath);
            }
            catch (Exception e)
            { 
                throw new Exception("The selected directory does not contain a git repository.");
            }
         

            RepositoryStatus gitStatus = GitRepo.RetrieveStatus(new StatusOptions() { IncludeUnaltered = true });

            //check if there are altered files (IS NOT ALLOWED)
            if (gitStatus.IsDirty)
            {
                //throw new Exception("Repository contains dirty files. Commit all changes and retry.");
            }

            initializeAuthors();

            List<string> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath).ToList();
            
            //create root folder
            var rootFolderName = Path.GetFileName(RootPath);
            var rootFolder = new PCOFolder(rootFolderName, null);
           
            foreach (string filePath in filePaths)
            {
                PCOFile addedFile = rootFolder.AddChildRecursive(filePath.Split("/"), 0);
                AddFileCommitsNonLibGit(addedFile, filePath);
            }

            return rootFolder;
        }


        private void AddFileCommits(PCOFile file, string filePath)
        {
            var blameHunkGroups = GitRepo.Blame(filePath).GroupBy(hunk => hunk.FinalCommit.Sha);

            foreach (var group in blameHunkGroups)
            {
                int commitLineCount = group.Sum(hunk => hunk.LineCount);
                var finalSignature = group.First().FinalSignature;
                var commit = new PCOCommit(finalSignature.Email, finalSignature.Name, finalSignature.When.Date);
                commit.AddLine(PCOCommit.LineType.NORMAL, commitLineCount);
                file.commits.Add(commit);
            }
        }


        private async void AddFileCommitsNonLibGit(PCOFile file, string filePath)
        {
            ProcessStartInfo processInfo;
            Process process;

            var commits = new Dictionary<string, PCOCommit>();

            processInfo = new ProcessStartInfo("cmd.exe", "/c git blame -e \"" + filePath + "\"");

            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            process = Process.Start(processInfo);

            CultureInfo provider = CultureInfo.InvariantCulture;

            var contributorManager = ContributorManager.GetInstance();

            StringBuilder sb = new StringBuilder();
            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                sb.AppendLine(e.Data);

                var line = e.Data;
                if (line == null) return;
                var match = GIT_BLAME_REGEX.Match(line);


                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var email = match.Groups[2].Value;
                    var datestring = match.Groups[3].Value;
                    var content = match.Groups[4].Value;
                    if (!commits.ContainsKey(key))
                    {
                        DateTime date = DateTime.ParseExact(datestring, "yyyy-mm-dd", provider);
                        commits.Add(key, new PCOCommit(email, contributorManager.GetAuthor(email).Name, date));
                    }

                    if (content.Trim().Length == 0)
                    {
                        commits[key].AddLine(PCOCommit.LineType.WHITE_SPACE);
                    } else
                    {
                        commits[key].AddLine(PCOCommit.LineType.NORMAL);
                    }

                }
            };


            process.BeginOutputReadLine();
            process.WaitForExit();

            file.commits = commits.Values.ToList();
                
            AddCreatorToFile(file, filePath);

        }

        private async void AddCreatorToFile(PCOFile file, string filePath)
        {
            var contributorManager = ContributorManager.GetInstance();

            var processInfo = new ProcessStartInfo("cmd.exe", "/c git log --diff-filter=A -- \"" + filePath + "\"");

            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            var process = Process.Start(processInfo);

            var sb = new StringBuilder();

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var match = AUTHOR_REGEX.Match(line.Trim());

                if (match.Success)
                {
                    sb.AppendLine(line);
                    var name = match.Groups[1].Value;
                    var email = match.Groups[2].Value;

                    file.Creator = contributorManager.GetAuthor(email);
                }
            };
            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                sb.AppendLine(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

        }

        private async void initializeAuthors()
        {
            var contributorManager = ContributorManager.GetInstance();

            var processInfo = new ProcessStartInfo("cmd.exe", "/c git log");

            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = processInfo.RedirectStandardError =  true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            var process = Process.Start(processInfo);

            var sb = new StringBuilder();

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var match = AUTHOR_REGEX.Match(line.Trim());

                if (match.Success)
                {
                    sb.AppendLine(line);
                    var name = match.Groups[1].Value;
                    var email = match.Groups[2].Value;

                    contributorManager.InitializeAuthor(email, name);
                }
            };
            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                sb.AppendLine(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

        }


        public PCOFolder AlternativeCollectAllData(string path)
        {
            RootPath = path;
            try
            {
                PCOState.GetInstance().TempGitRepo = new Repository(RootPath);
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
            var rootFolderName = Path.GetFileName(RootPath);
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
                var commit = new PCOCommit(finalSignature.Email, finalSignature.Name, finalSignature.When.Date);
                commit.AddLine(PCOCommit.LineType.NORMAL, commitLineCount);
                file.commits.Add(commit);
            }
        }

        public PCOFolder ParallelGetAllData(string path)
        {
            RootPath = path;
            try
            {
                GitRepo = new Repository(RootPath);
            }
            catch (Exception e)
            {
                throw new Exception("The selected directory does not contain a git repository.");
            }


            RepositoryStatus gitStatus = GitRepo.RetrieveStatus(new StatusOptions() { IncludeUnaltered = true });

            //check if there are altered files (IS NOT ALLOWED)
            if (gitStatus.IsDirty)
            {
                throw new Exception("Repository contains dirty files. Commit all changes and retry.");
            }

            initializeAuthors();

            List<string> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath).ToList();

            //create root folder
            var rootFolderName = Path.GetFileName(RootPath);
            var rootFolder = new PCOFolder(rootFolderName, null);

            //Debug.WriteLine(filePaths.Count());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Parallel.ForEach<string, PCOThreadData >(filePaths,
                new ParallelOptions() { MaxDegreeOfParallelism = 4},
                () =>new PCOThreadData(new PCOFolder(rootFolderName, null)),
                (filePath, loop, threadData) =>
                {
                    var threadRootFolder = threadData.ThreadRootFolder;
                    PCOFile addedFile = threadRootFolder.AddChildRecursive(filePath.Split("/"), 0);
                    AddFileCommitsNonLibGit(addedFile, filePath);
                    threadData.ThreadFileCount += 1;
                    return threadData;
                },
                (finalThreadData) => 
                { 
                    lock (RootFolderLock)
                    {
                        var finalThreadRootFolder = finalThreadData.ThreadRootFolder;
                        PCOFolderMergeHelper.MergeFolders(rootFolder, finalThreadRootFolder);

                        //TODO Add finalThreadData.ThreadFileCount to loading
                    }
                }
                );

            stopwatch.Stop();
            Debug.WriteLine("time taken: " + stopwatch.ElapsedMilliseconds / 1000 + " seconds");
            return rootFolder;
        }

        private class PCOThreadData
        {
            public PCOFolder ThreadRootFolder;
            public int ThreadFileCount;

            public PCOThreadData(PCOFolder f)
            {
                this.ThreadRootFolder = f;
                this.ThreadFileCount = 0;
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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.PowerShell.Commands;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Shapes;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.State;
using Windows.System;
using static System.Net.WebRequestMethods;
using Path = System.IO.Path;

namespace Project_Codebase_Overview.DataCollection
{
    internal class GitDataCollector : IVCSDataCollector
    {

        Repository GitRepo;
        string RootPath;
        private readonly object RootFolderLock = new object();
        private PCOFolder RootFolder;


        private static readonly Regex GIT_BLAME_REGEX = new Regex(@"([a-f0-9]+) .*\(<(.+)>[ ]+([0-9]{4}-[0-9]{2}-[0-9]{2}) [0-9]{2}:[0-9]{2}:[0-9]{2} [-\+]{0,1}[0-9}{4}[ ]+[0-9]\).*");
        private static readonly string GIT_BLAME_PS_REGEX = "(?<Key>[\\^]?[a-f0-9]+) .*\\([<](?<Email>.+)[>][ ]+(?<DateString>[0-9]{4}-[0-9]{2}-[0-9]{2}) [0-9]{2}:[0-9]{2}:[0-9]{2} [\\-\\+]{0,1}[0-9]{4}[ ]+[0-9]+\\).*";
        private static readonly Regex AUTHOR_REGEX = new Regex(@"Author: (.+) <(.+)>");
        private static readonly int PARALLEL_CHUNK_SIZE = 64;

        public async Task<PCOFolder> CollectAllData(string path)
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            await System.Threading.Tasks.Task.Run(() =>
            {
                //RootFolder = this.Simple2CollectAllData(path, dispatcherQueue);
                RootFolder = this.Parallel2GetAllData(path, dispatcherQueue);
                //PCOState.GetInstance().GetExplorerState().TestSetRootFolder(RootFolder);
            });

            //PCOState.GetInstance().GetTestState().PrintWatches();

            return RootFolder;

        }

        public PCOFolder SimpleCollectAllData(string path)
        // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
        // could be important to keep around for optimization testing
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
                AddFileCommitsCMD(addedFile, filePath);
            }

            return rootFolder;
        }

         public PCOFolder Simple2CollectAllData(string path, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
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


            //set loading
            
            dispatcherQueue?.TryEnqueue(() =>
            {
                PCOState.GetInstance().GetLoadingState().SetTotalFilesToLoad(filePaths.Count);
            });

            var stopwatch = new Stopwatch();

            //create root folder
            var rootFolderName = Path.GetFileName(RootPath);
            var rootFolder = new PCOFolder(rootFolderName, null);
            var threadDataTemp = new PCOGitThreadData(rootFolder, RootPath, dispatcherQueue);

            stopwatch.Start();

            foreach (string filePath in filePaths)
            {
                PCOFile addedFile = rootFolder.AddChildRecursive(filePath.Split("/"), 0);
                threadDataTemp.AddFileCommitsCMD(addedFile, filePath);
            }
            threadDataTemp.Invoke();



            stopwatch.Stop();
            Debug.WriteLine("time taken: " + stopwatch.ElapsedMilliseconds / 1000 + " seconds");

            return rootFolder;
        }


        private async void AddFileCommitsCMD(PCOFile file, string filePath)
        // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
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

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var match = GIT_BLAME_REGEX.Match(line);


                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var email = match.Groups[2].Value;
                    var datestring = match.Groups[3].Value;
                    if (!commits.ContainsKey(key))
                    {
                        DateTime date = DateTime.ParseExact(datestring, "yyyy-mm-dd", provider);
                        commits.Add(key, new PCOCommit(email, contributorManager.GetAuthor(email).Name, date));
                    }
                     commits[key].AddLine(PCOCommit.LineType.NORMAL);
                    

                }
            };


            process.BeginOutputReadLine();
            process.WaitForExit();

            file.commits = commits.Values.ToList();
                
            AddCreatorToFile(file, filePath);

        }

        private async void AddCreatorToFile(PCOFile file, string filePath)
        // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
        {
            return;
            var contributorManager = ContributorManager.GetInstance();

            var processInfo = new ProcessStartInfo("cmd.exe", "/c git log --diff-filter=A -- \"" + filePath + "\"");

            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var match = AUTHOR_REGEX.Match(line.Trim());

                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    var email = match.Groups[2].Value;

                    file.Creator = contributorManager.GetAuthor(email);
                }
            };

            process.BeginOutputReadLine();
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

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var match = AUTHOR_REGEX.Match(line.Trim());

                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    var email = match.Groups[2].Value;

                    contributorManager.InitializeAuthor(email, name);
                }
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

        }


        public PCOFolder AlternativeCollectAllData(string path)
        {
            // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
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

        public PCOFolder ParallelGetAllData(string path, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
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

            //set loading
            dispatcherQueue?.TryEnqueue(() =>
            {
                PCOState.GetInstance().GetLoadingState().SetTotalFilesToLoad(filePaths.Count);
            });
            

            //Debug.WriteLine(filePaths.Count());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Parallel.ForEach<string, PCOGitThreadData>(filePaths,
                new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                () => new(new PCOFolder(rootFolderName, null), RootPath, dispatcherQueue),
                (filePath, loop, threadData) =>
                {
                    var threadRootFolder = threadData.ThreadRootFolder;
                    PCOFile addedFile = threadRootFolder.AddChildRecursive(filePath.Split("/"), 0);

                    AddFileCommitsCMD(addedFile, filePath);

                    threadData.ThreadFileCount += 1;

                    return threadData;
                },
                (finalThreadData) =>
                {
                    finalThreadData.Invoke();


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

        public PCOFolder Parallel2GetAllData(string path, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, int parallelChunkSetter = 0)
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

            //set loading
            dispatcherQueue?.TryEnqueue(() =>
            {
                PCOState.GetInstance().GetLoadingState().SetTotalFilesToLoad(filePaths.Count);
            });
            

            // Testing parallel block size start TODO: remove parallelChunkSetter and use PARALLEL_CUNK_SIZE parameter instead
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (parallelChunkSetter == 0)
            {
                parallelChunkSetter = PARALLEL_CHUNK_SIZE;
            }

            var rangePartitioner = Partitioner.Create(0, filePaths.Count(), Math.Max(parallelChunkSetter, 1)); //Keep this line after testing is done
            if (parallelChunkSetter == -1)
            {
                rangePartitioner = Partitioner.Create(0, filePaths.Count());
            }
            // Testing End



            Parallel.ForEach(rangePartitioner,
                new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                (range, loop) =>
                {

                    var threadData = new PCOGitThreadData(new PCOFolder(rootFolderName, null), RootPath, dispatcherQueue);
                    var threadRootFolder = threadData.ThreadRootFolder;

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var filePath = filePaths[i];
                        PCOFile addedFile = threadRootFolder.AddChildRecursive(filePath.Split("/"), 0);

                        threadData.AddFileCommitsCMD(addedFile, filePath);

                        threadData.ThreadFileCount += 1;
                    }
                    threadData.Invoke();

                    lock (RootFolderLock)
                    {
                        var finalThreadRootFolder = threadData.ThreadRootFolder;
                        PCOFolderMergeHelper.MergeFolders(rootFolder, finalThreadRootFolder);
                    }
                });

            stopwatch.Stop();
            Debug.WriteLine("time taken: " + stopwatch.ElapsedMilliseconds / 1000 + " seconds");
            
            return rootFolder;
        }

        private class PCOGitThreadData
        {
            public PCOFolder ThreadRootFolder;
            public int ThreadFileCount;
            public int CurrentIndex;
            public int CurrentCreatorIndex;
            private ProcessStartInfo ProcessInfo;
            public Process Process;
            private ProcessStartInfo CreatorProcessInfo;
            public Process CreatorProcess;
            public List<PCOFile> Files;
            public Dictionary<string, PCOCommit> CurrentCommits;
            public List<string> Commands;
            public List<string> CreatorCommands;
            public bool IsGettingCreator;
            public Author CurrentCreator;

            public PCOGitThreadData(PCOFolder f, string rootPath, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
            {
                this.Commands = new List<string>();
                this.CreatorCommands = new List<string>();
                this.Files = new List<PCOFile>();
                this.CurrentCommits = new Dictionary<string, PCOCommit>();
                this.ThreadRootFolder = f;
                this.ThreadFileCount = 0;
                this.CurrentIndex = -1;
                this.CurrentCreatorIndex = -1;
                this.IsGettingCreator = false;

                this.ProcessInfo = new ProcessStartInfo("cmd.exe");

                ProcessInfo.RedirectStandardInput = ProcessInfo.RedirectStandardOutput = ProcessInfo.RedirectStandardError = true;
                ProcessInfo.CreateNoWindow = true;
                ProcessInfo.UseShellExecute = false;
                ProcessInfo.WorkingDirectory = rootPath;
                ProcessInfo.FileName = "cmd.exe";

                this.Process = new Process();
                this.Process.StartInfo = ProcessInfo;

                this.CreatorProcessInfo = new ProcessStartInfo("cmd.exe");

                CreatorProcessInfo.RedirectStandardInput = CreatorProcessInfo.RedirectStandardOutput = CreatorProcessInfo.RedirectStandardError = true;
                CreatorProcessInfo.CreateNoWindow = true;
                CreatorProcessInfo.UseShellExecute = false;
                CreatorProcessInfo.WorkingDirectory = rootPath;
                CreatorProcessInfo.FileName = "cmd.exe";

                this.CreatorProcess = new Process();
                this.CreatorProcess.StartInfo = CreatorProcessInfo;


                CultureInfo provider = CultureInfo.InvariantCulture;

                var contributorManager = ContributorManager.GetInstance();

                //this.Process.ErrorDataReceived += (sender, e) => { Debug.WriteLine("Error line from cmd: " + e.Data); };

                this.Process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    var line = e.Data;
                    if (line == null) return;
                    if (line.StartsWith("file-creator:"))
                    {
                        this.IsGettingCreator = true;
                        if (this.CurrentIndex != -1)
                        {
                            this.Files[this.CurrentIndex].commits = this.CurrentCommits.Values.ToList();
                            this.Files[this.CurrentIndex].Creator = this.CurrentCreator;
                            this.CurrentCommits = new Dictionary<string, PCOCommit>();
                            dispatcherQueue?.TryEnqueue(() =>
                            {
                                PCOState.GetInstance().GetLoadingState().AddFilesLoaded(1);
                            });
                        }

                        this.CurrentIndex++;
                    } else if (line.StartsWith("file-blame:")) 
                    {
                        this.IsGettingCreator = false;
                    }
                    else
                    {
                        if (this.IsGettingCreator)
                        {
                            
                            var match = AUTHOR_REGEX.Match(line.Trim());

                            if (match.Success)
                            {
                                var name = match.Groups[1].Value;
                                var email = match.Groups[2].Value;

                                this.CurrentCreator = contributorManager.GetAuthor(email);
                            }
                        } else
                        {
                            var match = GIT_BLAME_REGEX.Match(line);

                            if (match.Success)
                            {
                                var key = match.Groups[1].Value;
                                var email = match.Groups[2].Value;
                                var datestring = match.Groups[3].Value;
                                if (!CurrentCommits.ContainsKey(key))
                                {
                                    DateTime date = DateTime.ParseExact(datestring, "yyyy-mm-dd", provider);
                                    CurrentCommits.Add(key, new PCOCommit(email, contributorManager.GetAuthor(email).Name, date));
                                }
                                CurrentCommits[key].AddLine(PCOCommit.LineType.NORMAL);

                            }
                        }
                        
                    }

                };

                this.CreatorProcess.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    var line = e.Data;
                    if (line == null) return;
                    if (line.StartsWith("file-creator:"))
                    {
                        if (this.CurrentCreatorIndex != -1)
                        {
                            this.Files[this.CurrentCreatorIndex].Creator = this.CurrentCreator;
                        }

                        this.CurrentCreatorIndex++;
                    }
                    else
                    {
                        var match = AUTHOR_REGEX.Match(line.Trim());

                        if (match.Success)
                        {
                            var name = match.Groups[1].Value;
                            var email = match.Groups[2].Value;

                            this.CurrentCreator = contributorManager.GetAuthor(email);
                        }

                    }

                };

            }

            public async void AddFileCommitsCMD(PCOFile file, string filePath)
            {

                this.Files.Add(file);

                this.Commands.Add("echo file-creator:" + filePath);
                //this.Commands.Add("git log --diff-filter=A -- \"" + filePath + "\"");
                this.Commands.Add("echo file-blame:" + filePath);
                this.Commands.Add("git blame -e \"" + filePath + "\"");

                this.CreatorCommands.Add("echo file-creator:" + filePath);
                this.CreatorCommands.Add("git log --diff-filter=A -- \"" + filePath + "\"");

            }

            public async void Invoke()
            {
                this.Commands.Add("echo file-creator:Done");
                this.CreatorCommands.Add("echo file-creator:Done");

                this.Process.Start();
                this.Process.BeginOutputReadLine();
                this.Process.BeginErrorReadLine();

                using (StreamWriter sw = this.Process.StandardInput)
                {
                    foreach (var cmd in this.Commands)
                    {
                        sw.WriteLine(cmd);
                    }
                    Debug.WriteLine("Thead with " + (this.Commands.Count() - 1) / 3 + " files Completed");

                }

                this.Process.WaitForExit();


                //this.CreatorProcess.Start();
                //this.CreatorProcess.BeginOutputReadLine();
                //this.CreatorProcess.BeginErrorReadLine();

                //using (StreamWriter sw = this.CreatorProcess.StandardInput)
                //{
                //    foreach (var cmd in this.CreatorCommands)
                //    {
                //        sw.WriteLine(cmd);
                //    }

                //}

                //this.CreatorProcess.WaitForExit();
            }

        }

        public void testTime()
        // Depricated.. Used for testing. TODO: remove when testing no longer nessesary
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

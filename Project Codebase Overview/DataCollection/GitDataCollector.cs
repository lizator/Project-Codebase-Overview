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
using System.Management.Automation.Language;
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
        public static readonly Regex AUTHOR_REGEX = new Regex(@"Author: (.+) <(.+)>");
        public static readonly Regex AUTHOR_REGEX_2 = new Regex(@"Commit - (.+) - (.+) - (.+)");
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
        public async Task<PCOFolder> CollectNewData(string path, PCOFolder oldDataRootFolder, string lastLoadedCommitSHA)
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            await System.Threading.Tasks.Task.Run(() =>
            {
                RootFolder = this.Parallel2GetNewData(path, dispatcherQueue, oldDataRootFolder, lastLoadedCommitSHA);
            });

            return RootFolder;

        }


        public RepositoryStatus GetRepoStatus(string path)
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
            return gitStatus;
        }

        private void InitializeNewAuthorsAndCreators(List<string> changedFilePaths, string lastCommitSha)
        {
            var contributorState = PCOState.GetInstance().GetContributorState();

            var isPathNew = new Dictionary<string, bool>();
            foreach (var changedPath in changedFilePaths)
            {
                isPathNew.Add(changedPath, !RootFolder.IsPathInitialized(changedPath));
            }
            var currentEmail = "";
            var maxFilepathLength = 0;
            foreach (var path in changedFilePaths)
            {
                maxFilepathLength = Math.Max(maxFilepathLength, path.Length);
            }


            var processInfo = new ProcessStartInfo("cmd.exe", "/c git log --pretty=format:\"Commit - %h - %ae - %an\" --stat=" + maxFilepathLength * 2 + "," + maxFilepathLength * 2 + " " + lastCommitSha + "..HEAD");

            // git log --pretty=format:"Commit - %h - %ae - %an" --stat=1000,1000 bdf1db970a616b0bede045a5160f50dcdc2f0fdd..HEAD


            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var match = AUTHOR_REGEX_2.Match(line.Trim());

                if (match.Success)
                {
                    var name = match.Groups[3].Value;
                    currentEmail = match.Groups[2].Value;

                    contributorState.InitializeAuthor(currentEmail, name, null);
                }
                else if (line.Trim().Length > 0)
                {
                    var split = line.Split('|');
                    if (split.Length == 2)
                    {
                        var currPath = split[0].Trim();

                        while (currPath.Contains('{') && currPath.Contains('}') && currPath.Contains("=>"))
                        {
                            var firstIndex = currPath.IndexOf('{');
                            var lastIndex = currPath.IndexOf('}');
                            var str = currPath.Substring(firstIndex, lastIndex - firstIndex + 1);
                            var lastStr = str.Split("=>")[1].Trim();
                            var newSubString = lastStr.Substring(0, lastStr.Length - 1);
                            currPath = currPath.Replace(str, newSubString);
                        }
                        if (isPathNew[currPath])
                        {
                            contributorState.UpdateCreator(currPath, currentEmail);
                        }
                    }
                }
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

        }
        public PCOFolder Parallel2GetNewData(string path, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, PCOFolder oldDataRootFolder, string lastLoadedCommitSHA)
        {
            /* For loading changes when loading state from save-file. Loads changes since last save. */

            var gitStatus = GetRepoStatus(path);


            //Save latest commit sha to state
            var latestCommitSha = GitRepo.Commits.First().Sha;
            if (latestCommitSha.Equals(lastLoadedCommitSHA))
            {
                return oldDataRootFolder;
            }

            PCOState.GetInstance().SetLatestCommitSha(latestCommitSha);

            //get filepaths
            var processInfo = new ProcessStartInfo("cmd.exe", "/c git diff --numstat " + lastLoadedCommitSHA + " HEAD");

            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            var process = Process.Start(processInfo);

            List<string> changedFilePaths = new List<string>();

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var regex = new Regex("[0-9]+\t[0-9]+\t(.+)");
                var match = regex.Match(line);
                // AUTHOR_REGEX.Match(line.Trim());

                if (match.Success)
                {
                    var pathMatch = match.Groups[1].Value;
                    changedFilePaths.Add(pathMatch);
                }
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

            //find files that are new since last load
            InitializeNewAuthorsAndCreators(changedFilePaths, lastLoadedCommitSHA);

            //create root folder
            var rootFolderName = Path.GetFileName(RootPath);
            var rootFolder = new PCOFolder(rootFolderName, null);

            //set loading
            dispatcherQueue?.TryEnqueue(() =>
            {
                PCOState.GetInstance().GetLoadingState().SetTotalFilesToLoad(changedFilePaths.Count);
            });

            var rangePartitioner = Partitioner.Create(0, changedFilePaths.Count(), Math.Max(PARALLEL_CHUNK_SIZE, 1)); //Keep this line after testing is done


            Parallel.ForEach(rangePartitioner,
                new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                (range, loop) =>
                {

                    var threadData = new PCOGitThreadData(new PCOFolder(rootFolderName, null), RootPath, dispatcherQueue);
                    var threadRootFolder = threadData.ThreadRootFolder;

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var filePath = changedFilePaths[i];
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

            var updatedRoot = PCOFolderMergeHelper.MergeFolders(oldDataRootFolder, rootFolder);
            return updatedRoot;
        }



        private async void initializeAuthorsAndCreators(List<string> filepaths)
        {
            var contributorState = PCOState.GetInstance().GetContributorState();

            var currentEmail = "";
            var maxFilepathLength = 0;
            foreach (var path in filepaths)
            {
                maxFilepathLength = Math.Max(maxFilepathLength, path.Length);
            }


            var processInfo = new ProcessStartInfo("cmd.exe", "/c git log --pretty=format:\"Commit - %h - %ae - %an\" --stat=" + maxFilepathLength*2 + "," + maxFilepathLength*2  );

            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = processInfo.RedirectStandardError =  true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {

                var line = e.Data;
                if (line == null) return;
                var match = AUTHOR_REGEX_2.Match(line.Trim());

                if (match.Success)
                {
                    var name = match.Groups[3].Value;
                    currentEmail = match.Groups[2].Value;

                    contributorState.InitializeAuthor(currentEmail, name, null);
                } else if (line.Trim().Length > 0)
                {
                    var split = line.Split('|');
                    if (split.Length == 2)
                    {
                        var currPath = split[0].Trim();

                        while (currPath.Contains('{') && currPath.Contains('}') && currPath.Contains("=>"))
                        {
                            var firstIndex = currPath.IndexOf('{');
                            var lastIndex = currPath.IndexOf('}');
                            var str = currPath.Substring(firstIndex, lastIndex - firstIndex + 1);
                            var lastStr = str.Split("=>")[1].Trim();
                            var newSubString = lastStr.Substring(0, lastStr.Length - 1);
                            currPath = currPath.Replace(str, newSubString);
                        }

                        contributorState.UpdateCreator(currPath, currentEmail);
                    }
                }
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

        }

        

        public PCOFolder Parallel2GetAllData(string path, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, int parallelChunkSetter = 0)
        {
            var gitStatus = GetRepoStatus(path);

            //Save latest commit sha to state
            var latestCommit = GitRepo.Commits.First();
            PCOState.GetInstance().SetLatestCommitSha(latestCommit.Sha);
            PCOState.GetInstance().SetBranchName(GitRepo.Head.FriendlyName);

            //initializeAuthors();

            List<string> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath).ToList();

            initializeAuthorsAndCreators(filePaths);

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

        public bool DoesBranchContainLastCommit()
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c git branch --contains " + PCOState.GetInstance().GetLatestCommitSha());

            processInfo.RedirectStandardInput = processInfo.RedirectStandardOutput = processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = RootPath;

            var process = Process.Start(processInfo);

            var containsList = new List<string>();
            var hasError = false;
            var errorList = new List<string>();

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                var line = e.Data;
                if (line != null)
                {
                    if (line.StartsWith('*'))
                    {
                        line = line.Substring(1);
                    }
                    var name = line.Trim();
                    containsList.Add(name);
                }
            };

            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                var line = e.Data;
                errorList.Add(line);
                hasError = true;
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

            return !hasError && containsList.Contains(PCOState.GetInstance().GetBranchName());
        }

        private class PCOGitThreadData
        {
            public PCOFolder ThreadRootFolder;
            public int ThreadFileCount;
            public int CurrentIndex;
            public int CurrentCreatorIndex;
            private ProcessStartInfo ProcessInfo;
            public Process Process;
            public List<PCOFile> Files;
            public Dictionary<string, PCOCommit> CurrentCommits;
            public List<string> Commands;
            public String CurrentPath;

            public PCOGitThreadData(PCOFolder f, string rootPath, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
            {
                this.Commands = new List<string>();
                this.Files = new List<PCOFile>();
                this.CurrentCommits = new Dictionary<string, PCOCommit>();
                this.ThreadRootFolder = f;
                this.ThreadFileCount = 0;
                this.CurrentIndex = -1;
                this.CurrentPath = "";

                this.ProcessInfo = new ProcessStartInfo("cmd.exe");

                ProcessInfo.RedirectStandardInput = ProcessInfo.RedirectStandardOutput = ProcessInfo.RedirectStandardError = true;
                ProcessInfo.CreateNoWindow = true;
                ProcessInfo.UseShellExecute = false;
                ProcessInfo.WorkingDirectory = rootPath;
                ProcessInfo.FileName = "cmd.exe";

                this.Process = new Process();
                this.Process.StartInfo = ProcessInfo;

                CultureInfo provider = CultureInfo.InvariantCulture;

                var contributorState = PCOState.GetInstance().GetContributorState();

                //this.Process.ErrorDataReceived += (sender, e) => { Debug.WriteLine("Error line from cmd: " + e.Data); };

                this.Process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    var line = e.Data;
                    if (line == null) return;
                    if (line.StartsWith("file:"))
                    {
                        if (this.CurrentIndex != -1)
                        {
                            this.Files[this.CurrentIndex].commits = this.CurrentCommits.Values.ToList();
                            this.Files[this.CurrentIndex].Creator = contributorState.GetCreatorFromPath(this.CurrentPath);
                            if (this.Files[this.CurrentIndex].Creator == null)
                            {
                                Debug.WriteLine("Could not find creator by currentPath: \"" + this.CurrentPath + "\"");
                            }
                            this.CurrentCommits = new Dictionary<string, PCOCommit>();
                            dispatcherQueue?.TryEnqueue(() =>
                            {
                                PCOState.GetInstance().GetLoadingState().AddFilesLoaded(1);
                            });
                        }
                        this.CurrentPath = line.Split(':')[1];
                        this.CurrentIndex++;
                    } 
                    else
                    {
                        var match = GIT_BLAME_REGEX.Match(line);

                        if (match.Success)
                        {
                            var key = match.Groups[1].Value;
                            var email = match.Groups[2].Value;
                            var datestring = match.Groups[3].Value;
                            if (!CurrentCommits.ContainsKey(key))
                            {
                                DateTime date = DateTime.ParseExact(datestring, "yyyy-MM-dd", provider);
                                CurrentCommits.Add(key, new PCOCommit(email, date));
                            }
                            CurrentCommits[key].AddLine(PCOCommit.LineType.NORMAL);

                        }
                    }

                };

            }

            public async void AddFileCommitsCMD(PCOFile file, string filePath)
            {

                this.Files.Add(file);

                this.Commands.Add("echo file:" + filePath);
                this.Commands.Add("git blame -e \"" + filePath + "\"");

            }

            public async void Invoke()
            {
                this.Commands.Add("echo file:Done");

                this.Process.Start();
                this.Process.BeginOutputReadLine();
                this.Process.BeginErrorReadLine();

                using (StreamWriter sw = this.Process.StandardInput)
                {
                    foreach (var cmd in this.Commands)
                    {
                        sw.WriteLine(cmd);
                    }
                    //Debug.WriteLine("Thread with " + (this.Commands.Count() - 1) / 2 + " files Completed");

                }
                this.Process.WaitForExit();
            }

        }

        #region Depricated
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
            //Depricated...
            var blameHunkGroups = PCOState.GetInstance().TempGitRepo.Blame(filePath).GroupBy(hunk => hunk.FinalCommit.Id);

            foreach (var group in blameHunkGroups)
            {
                int commitLineCount = group.Sum(hunk => hunk.LineCount);
                var finalSignature = group.First().FinalSignature;
                var commit = new PCOCommit(finalSignature.Email, finalSignature.When.Date);
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

            //initializeAuthors();

            List<string> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath).ToList();

            initializeAuthorsAndCreators(filePaths);

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
                    //finalThreadData.Invoke();


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

            //initializeAuthors();

            List<string> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath).ToList();

            initializeAuthorsAndCreators(filePaths);

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

            var contributorState = PCOState.GetInstance().GetContributorState();

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
                        commits.Add(key, new PCOCommit(email, date));
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
            var contributorState = PCOState.GetInstance().GetContributorState();

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

                    file.Creator = contributorState.GetAuthor(email);
                }
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

        }

        private async void initializeAuthors()
        {
            var contributorState = PCOState.GetInstance().GetContributorState();

            var processInfo = new ProcessStartInfo("cmd.exe", "/c git log");

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

                    contributorState.InitializeAuthor(email, name, null);
                }
            };

            process.BeginOutputReadLine();
            process.WaitForExit();

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
                if (i % 10 == 0)
                {
                    Debug.WriteLine(i);
                }
            }
            stopwatch.Stop();

            Debug.WriteLine("Average elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds / repetitions);

            /*
            ALL PROGRAMS CLOSED
            Results:
            -Version 1, average time 1746
            -Using sha instead of id for groouping of commits, time 1723
            -version 2(creating breadth first ish setup) time 1768

            */

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


            //initializeAuthors();

            List<string> filePaths = gitStatus.Unaltered.Select(statusEntry => statusEntry.FilePath).ToList();

            initializeAuthorsAndCreators(filePaths);

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

        
        #endregion
    }
}

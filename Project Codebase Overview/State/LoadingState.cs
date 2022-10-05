using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Automation.Peers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Project_Codebase_Overview.State
{

    
    public class LoadingState : ObservableObject
    {
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        private bool _isLoading;
        public int TotalFilesToLoad
        {
            get => _totalFilesToLoad;
            set => SetProperty(ref _totalFilesToLoad, value);
        }
        private int _totalFilesToLoad;

        public int FilesLoaded
        {
            get => _filesLoaded;
            set => SetProperty(ref _filesLoaded, value);
        }
        private int _filesLoaded;
        public double PercentageDone
        {
            get => _percentageDone;
            set => SetProperty(ref _percentageDone, value);
        }
        private double _percentageDone;

        public LoadingState()
        {
            _filesLoaded = 0;
            _percentageDone = 0;
            _isLoading = true;
            _totalFilesToLoad = 100;
        }

        public void SetTotalFilesToLoad(int total)
        {
            TotalFilesToLoad = total;
        }
        public void AddFilesLoaded(int additionalFilesLoaded)
        {
            FilesLoaded += additionalFilesLoaded;
            PercentageDone = (double)FilesLoaded / (double)TotalFilesToLoad * 100;
        }

        public async Task<object> TestLoading()
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            await System.Threading.Tasks.Task.Run(() =>
            {
                PercentageDone = 0;
                FilesLoaded = 0;
                IsLoading = true;
                for (int i = 0; i < 10; i++)
                {
                    System.Threading.Thread.Sleep(700);
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        this.AddFilesLoaded(10);
                    });

                }
            });
            return null;
        }


    }
}

using LibGit2Sharp;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.LocalSettings;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartWindow : Window
    {
        ObservableCollection<RecentFileInfo> RecentFiles;
        public StartWindow()
        {
            this.InitializeComponent();

            RecentFiles = LocalSettingsHelper.GetRecentFiles().ToObservableCollection();
        }


        private async void SelectFolder(object sender, RoutedEventArgs e)
        {
            PCOState.GetInstance().ClearState();

            var folderPicker = new FolderPicker();

            IntPtr windowHandler = WinRT.Interop.WindowNative.GetWindowHandle(this);
            folderPicker.FileTypeFilter.Add("*"); // work around to fix the crash
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, windowHandler);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }

            try
            {
                //test if repo available and nondirty
                GitDataCollector collector = new GitDataCollector();
                collector.GetRepoStatus(folder.Path);
            }
            catch (Exception ex)
            {
                await DialogHandler.ShowErrorDialog(ex.Message, this.Content.XamlRoot);
                return;
            }

            PCOState.GetInstance().GetExplorerState().SetRootPath(folder.Path);

            NavigateToLoadingPage();
        }

        private void NavigateToExplorerPage()
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            mainWindow.NavigateToExplorerPage();
            Close();
        }
        private void NavigateToLoadingPage()
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            mainWindow.NavigateToLoadingPage();
            Close();
        }
        private void NavigateToLoadingSavePage(StorageFile file)
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            mainWindow.NavigateToLoadingSavePage(new LoadingSavePageParameters(file));
            Close();
        }

        private async void LoadFileClick(object sender, RoutedEventArgs e)
        {
            PCOState.GetInstance().ClearState();

            var filePicker = new FileOpenPicker();
            IntPtr windowHandler = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, windowHandler);
            filePicker.FileTypeFilter.Add(".json");
            var file = await filePicker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            NavigateToLoadingSavePage(file);

        }

        private async void RecentFileClick(object sender, ItemClickEventArgs e)
        {
            PCOState.GetInstance().ClearState();
            var fileInfo = e.ClickedItem as RecentFileInfo;
            StorageFile file = null;
            try
            {
                file = await StorageFile.GetFileFromPathAsync(fileInfo.FilePath);
            }
            catch (Exception ex)
            {
                await DialogHandler.ShowErrorDialog(ex.Message, Content.XamlRoot);
            }
            NavigateToLoadingSavePage(file);
        }
    }
}

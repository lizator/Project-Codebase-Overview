using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.State;
using System.Threading.Tasks;
using System.Threading;
using Project_Codebase_Overview.Dialogs;
using Windows.Storage.Pickers;
using LibGit2Sharp;
using Windows.ApplicationModel.VoiceCommands;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestStartWindow : Window
    {
        public TestStartWindow()
        {
            this.InitializeComponent();
        }

        private async void UseAsIntended(object sender, RoutedEventArgs e)
        {
            PCOState.GetInstance().ClearState();
            StartWindow window = new();
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            OverlappedPresenter presenter = appWindow.Presenter as OverlappedPresenter;

            appWindow.Resize(new Windows.Graphics.SizeInt32(700, 500));
            presenter.IsResizable = false;

            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
            if (displayArea is not null)
            {
                var CenteredPosition = appWindow.Position;
                CenteredPosition.X = ((displayArea.WorkArea.Width - appWindow.Size.Width) / 2);
                CenteredPosition.Y = ((displayArea.WorkArea.Height - appWindow.Size.Height) / 2);
                appWindow.Move(CenteredPosition);
            }

            window.Activate();
        }

        private void NavigateToExplorerPage()
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            mainWindow.NavigateToExplorerPage();
        }
        private void NavigateToLoadingPage()
        {
            MainWindow window = new MainWindow();
            window.Activate();
            window.NavigateToLoadingPage();
        }

        private async void OpenPCOMaster(object sender, RoutedEventArgs e)
        {
            PCOState.GetInstance().ClearState();

            var state = PCOState.GetInstance();
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            state.GetExplorerState().SetRootPath(path);

            NavigateToLoadingPage();
        }

        private async void OpenDummyData(object sender, RoutedEventArgs e)
        {

            var state = PCOState.GetInstance();
            state.GetExplorerState().LoadRootFolder(loadDummyData: true);

            NavigateToExplorerPage();
        }

        private async void OpenDummyDataMerge(object sender, RoutedEventArgs e)
        {

            var state = PCOState.GetInstance();
            state.GetExplorerState().LoadRootFolder(loadDummyData: true, withMerge: true);

            NavigateToExplorerPage();
        }

        private async void RunAltGetData(object sender, RoutedEventArgs e)
        {
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            var collector = new GitDataCollector();
            var rootFolder = collector.AlternativeCollectAllData(path);
            PCOState.GetInstance().GetExplorerState().TestSetRootFolder(rootFolder);

            NavigateToExplorerPage();
        }


        private async void RunAltGetDataMerge(object sender, RoutedEventArgs e)
        {
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            var collector = new GitDataCollector();
            var rootFolder = collector.ParallelGetAllData(path, null);
            PCOState.GetInstance().GetExplorerState().TestSetRootFolder(rootFolder);

            NavigateToExplorerPage();
        }

        private void start_test(object sender, RoutedEventArgs e)
        {
            GitDataCollector collector = new GitDataCollector();
            collector.testTime();
        }

        private void TestLoadingClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            mainWindow.NavigateToLoadingPage();
        }
        private async void TestLoadingIntended(object sender, RoutedEventArgs e)
        {
            PCOState.GetInstance().ClearState();

            var folderPicker = new FolderPicker();

            IntPtr windowHandler = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, windowHandler);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }

            try
            {
                //test if repo available
                var testingRepo = new Repository(folder.Path);
            }
            catch (Exception ex)
            {
                await DialogHandler.ShowErrorDialog("The selected directory does not contain a git repository.", this.Content.XamlRoot);
                return;
            }

            PCOState.GetInstance().GetExplorerState().SetRootPath(folder.Path);
            
            MainWindow window = new MainWindow();
            window.Activate();
            window.NavigateToLoadingPage();
        }
    }
}

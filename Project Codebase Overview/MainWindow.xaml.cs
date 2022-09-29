using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

        private void start_test(object sender, RoutedEventArgs e)
        {
            GitDataCollector collector = new GitDataCollector();
            collector.testTime();
        }

        private async void UseAsIntended(object sender, RoutedEventArgs e)
        {
            var fp = new FolderPicker();

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(fp, hwnd);

            var folder = await fp.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }


            try
            {
                var state = PCOState.GetInstance();
                state.GetExplorerState().SetRootPath(folder.Path, forceReload: true);
            } catch (Exception ex)
            {
                DialogHandler.ShowErrorDialog(ex.Message, this.Content.XamlRoot);
                return;
            }

            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }

        private async void OpenPCOMaster(object sender, RoutedEventArgs e)
        {


            var state = PCOState.GetInstance();
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            state.GetExplorerState().SetRootPath(path, forceReload: true);

            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }

        private async void OpenDummyData(object sender, RoutedEventArgs e)
        {

            var state = PCOState.GetInstance();
            state.GetExplorerState().LoadRootFolder(loadDummyData: true);

            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }

        private async void OpenDummyDataMerge(object sender, RoutedEventArgs e)
        {

            var state = PCOState.GetInstance();
            state.GetExplorerState().LoadRootFolder(loadDummyData: true, withMerge: true);

            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }

        private async void RunAltGetData(object sender, RoutedEventArgs e)
        {
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            var collector = new GitDataCollector();
            var rootFolder = collector.AlternativeCollectAllData(path);
            PCOState.GetInstance().GetExplorerState().TestSetRootFolder(rootFolder);
            

            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }   


        private async void RunAltGetDataMerge(object sender, RoutedEventArgs e)
        {
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            var collector = new GitDataCollector();
            var rootFolder = collector.ParallelGetAllData(path);
            PCOState.GetInstance().GetExplorerState().TestSetRootFolder(rootFolder);
            

            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }   
    }
}

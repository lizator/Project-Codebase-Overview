using Microsoft.UI;
using Microsoft.UI.Windowing;
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
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using AppWindow = Microsoft.UI.Windowing.AppWindow;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public MainWindow()
        {
            Instance = this;
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
            //SelectFolder();
        }


        public async void SelectFolder()
        {
            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }

        private async void OpenPCOMaster(object sender, RoutedEventArgs e)
        {


            var state = PCOState.GetInstance();
            var path = "C:\\TestRepos\\Project-Codebase-Overview";
            await state.GetExplorerState().SetRootPath(path, forceReload: true);

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

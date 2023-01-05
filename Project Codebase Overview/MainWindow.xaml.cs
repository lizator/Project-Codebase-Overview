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
using Syncfusion.UI.Xaml.Data;
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
using Windows.UI.Core;
using AppWindow = Microsoft.UI.Windowing.AppWindow;
using Windows.System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        bool controlKeyDown = false;
        public MainWindow()
        {
            this.InitializeComponent();
            (Application.Current as App)?.SetMainWindow(this);

            //fullscreen
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            PInvoke.User32.ShowWindow(hWnd, PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
        }

        public async void NavigateToLoadingPage()
        {
            this.NavigationFrame.Navigate(typeof(LoadingPage));
        }

        public async void NavigateToExplorerPage()
        {
            this.NavigationFrame.Navigate(typeof(ExplorerNavigationPage));
        }

        public async void NavigateToManagementPage()
        {
            this.NavigationFrame.Navigate(typeof(ManagementPage));
        }
        public async void NavigateToLoadingSavePage(LoadingSavePageParameters parameters)
        {
            this.NavigationFrame.Navigate(typeof(LoadingSavePage), parameters);

        }

        private void NavigationFrame_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
            {
                controlKeyDown = false;
            }
            else if (e.Key == VirtualKey.Z && controlKeyDown && PCOState.GetInstance().ChangeHistory.UndoAvailable)
            {
                PCOState.GetInstance().ChangeHistory.Undo();
            }
            else if (e.Key == VirtualKey.Y && controlKeyDown && PCOState.GetInstance().ChangeHistory.RedoAvailable)
            {
                PCOState.GetInstance().ChangeHistory.Redo();
            }
        }

        private void NavigationFrame_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == VirtualKey.Control)
            {
                controlKeyDown = true;
            }
        }

        public async void ShowToast(string text)
        {
            ToastHolder.Subtitle = text;
            ToastHolder.IsOpen = true;
            await Task.Delay(2000);
            ToastHolder.IsOpen = false;
        }
    }
}

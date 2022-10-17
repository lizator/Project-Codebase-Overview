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
        public MainWindow()
        {
            this.InitializeComponent();
        }

        public async void NavigateToExplorerPage()
        {
            (Application.Current as App)?.SetMainWindow(this);

            var rootFrame = new Frame();
            this.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }
        public async void NavigateToLoadingPage()
        {
            (Application.Current as App)?.SetMainWindow(this);
            
            var rootFrame = new Frame();
            this.MainFrame.Content = rootFrame;
            rootFrame.Navigate(typeof(LoadingPage));
        }
    }
}

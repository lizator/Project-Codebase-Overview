using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.DataCollection;
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
        private async void select_Path(object sender, RoutedEventArgs e)
        {
            var fp = new FolderPicker();

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(fp, hwnd);

            var folder = await fp.PickSingleFolderAsync();

            myText.Text = "Chosen path: " + folder.Path;

            GitDataCollector gitDataCollector = new GitDataCollector();
            gitDataCollector.CollectAllData(folder.Path);
        }

        private async void open_Explorer(object sender, RoutedEventArgs e)
        {
            var rootFrame = new Frame();
            var window = (Application.Current as App)?.window as MainWindow;
            window.Content = rootFrame;
            rootFrame.Navigate(typeof(ExplorerPage));
        }
    }
}

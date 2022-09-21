using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.TestDocs;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Syncfusion.UI.Xaml.TreeGrid;
using System.Collections.ObjectModel;
using Project_Codebase_Overview.FileExplorerView;
using Project_Codebase_Overview.State;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Microsoft.UI.Xaml.Automation.Peers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExplorerPage : Page
    {
        ExplorerViewModel viewModel;
        public ExplorerPage()
        {

            this.InitializeComponent();

            viewModel = (ExplorerViewModel)this.DataContext;

            viewModel.SetExplorerItems(PCOState.GetInstance().GetExplorerState().GetRoot());

            GetCurrentRoot();
        }

        private void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine("imageFailed: Exception: " + e.ErrorMessage);
        }

        private void GetCurrentRoot()
        {
            pathText.Text = PCOState.GetInstance().GetExplorerState().GetRootPath();
            
        }

        private void ExpandClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Expand clicked");
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Back clicked");
        }

        private void ForwardClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Forward clicked");
        }

        private void UpClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Up clicked");
        }
    }
}

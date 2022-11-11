using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.FileExplorerView;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoadingPage : Page
    {
        
        public LoadingPage()
        {
            //init component
            this.InitializeComponent();
            RepoNameText.Text = PCOState.GetInstance().GetExplorerState().GetRootPath();
            this.DataContext = PCOState.GetInstance().GetLoadingState();
            PCOState.GetInstance().GetLoadingState().PropertyChanged += LoadingStatePropertyChanged;

            //start load
            PCOState.GetInstance().GetExplorerState().InitializeRoot();
        }

        private void LoadingStatePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsLoading"))
            { 
                if (!PCOState.GetInstance().GetLoadingState().IsLoading)
                {
                    ((Application.Current as App)?.MainWindow as MainWindow).NavigateToExplorerPage();
                }
            }
            else if (e.PropertyName.Equals("TotalFilesToLoad"))
            {
                FilesLoadedLine.Visibility = Visibility.Visible;
                ScanningTextBlock.Visibility = Visibility.Collapsed;
            }
        }
    }
}

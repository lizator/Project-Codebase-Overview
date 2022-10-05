using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartWindow : Window
    {
        public StartWindow()
        {
            this.InitializeComponent();
        }


        private async void SelectFolder(object sender, RoutedEventArgs e)
        {
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
                var state = PCOState.GetInstance();
                await state.GetExplorerState().SetRootPath(folder.Path, forceReload: true);
            }
            catch (Exception ex)
            {
                await DialogHandler.ShowErrorDialog(ex.Message, this.Content.XamlRoot);
                return;
            }

            NavigateToExplorerPage();
            Close();
        }

        private void NavigateToExplorerPage()
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            mainWindow.NavigateToExplorerPage();
        }
    }
}

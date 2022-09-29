﻿using Microsoft.UI.Xaml;
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
using Project_Codebase_Overview.Dialogs;
using Windows.Storage.Pickers;

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

            rootTreeGrid.SelectionChanged += sfTreeGrid_SelectionChanged;

        }

        private void sfTreeGrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grids.GridSelectionChangedEventArgs e)
        {
            Debug.WriteLine("selection changed!");
            MenuFlyout menuFlyout = new MenuFlyout();
            var newlySelected = ((TreeGridRowInfo)e.AddedItems.First()).RowData as ExplorerItem;
            if (newlySelected.GetType() == typeof(PCOFolder))
            {
                MenuFlyoutItem setRootItem = new MenuFlyoutItem() { Text = "Set as root" };
                setRootItem.Click += this.SetRoot;
                menuFlyout.Items.Add(setRootItem);
            }
            else
            {
                //any right click functions for FILES
            }
            rootTreeGrid.ContextFlyout = menuFlyout;
        }
        

        private void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine("imageFailed: Exception: " + e.ErrorMessage);
        }

        private void ExpandClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Expand clicked");
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Back clicked");
            viewModel.NavigateBack();
        }

        private void ForwardClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Forward clicked");
            viewModel.NavigateForward();
        }

        private void UpClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Up clicked");
            viewModel.NavigateUp();
        }

        private void SetRoot(object sender, RoutedEventArgs e)
        {
            var selectedItem = rootTreeGrid.SelectedItem as ExplorerItem;
            Debug.WriteLine("set new root folder = " + selectedItem.Name);
            if (selectedItem.GetType() == typeof(PCOFolder))
            {
                Debug.WriteLine("this is folder");
                viewModel.SetNewRoot((PCOFolder) selectedItem);
            }
            else
            {
                Debug.WriteLine("this is file");
            }

        }


        
        private async void PathClick(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            IntPtr windowHandler = WinRT.Interop.WindowNative.GetWindowHandle( (Application.Current as App)?.window as MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, windowHandler);

            var selectedFolder = await folderPicker.PickSingleFolderAsync();
            if (selectedFolder == null)
            {
                return;
            }


            try
            {
                string rootpath = PCOState.GetInstance().GetExplorerState().GetRootPath();
                if (selectedFolder.Path.StartsWith(rootpath))
                {
                    viewModel.NavigateToPath(selectedFolder.Path);
                }
            }
            catch (Exception ex)
            {
                DialogHandler.ShowErrorDialog(ex.Message, this.Content.XamlRoot);
                return;
            }
        }
    }
}

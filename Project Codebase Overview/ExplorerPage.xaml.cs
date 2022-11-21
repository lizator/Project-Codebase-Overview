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
using System.Collections.ObjectModel;
using Project_Codebase_Overview.FileExplorerView;
using Project_Codebase_Overview.State;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Microsoft.UI.Xaml.Automation.Peers;
using Project_Codebase_Overview.Dialogs;
using Windows.Storage.Pickers;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.Graphs;
using Microsoft.UI.Xaml.Shapes;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.ContributorManagement;
using Syncfusion.UI.Xaml.Charts;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using Syncfusion.UI.Xaml.Editors;
using Project_Codebase_Overview.ChangeHistoryFolder;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExplorerPage : Page
    {
        ExplorerViewModel ViewModel;


        bool GraphViewActive = false;
        public ExplorerPage()
        {
            this.InitializeComponent();

            ViewModel = (ExplorerViewModel)this.DataContext;

            var root = PCOState.GetInstance().GetExplorerState().GetRoot();

            ViewModel.SetExplorerItems(root);
            ViewModel.SelectedGraphItem = root;

            rootTreeGrid.SelectionChanged += sfTreeGrid_SelectionChanged;

            PCOState.GetInstance().ChangeHistory.PropertyChanged += ChangeHistory_PropertyChanged;

            ViewModel.navButtonValues.PropertyChanged += NavButtonPropertyChanged;

            if (!PCOState.GetInstance().GetSettingsState().IsDecayActive)
            {
                rootTreeGrid.Columns.RemoveAt(4); //Remove decay column
            }

            ViewModel.UpdateBreadcrumbBar();
        }
        

        private void NavButtonPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("NavigateUpAvailable"))
            {
                if (((NavigationButtonValues)sender).NavigateUpAvailable)
                {
                    UpButton.IsEnabled = true;
                    UpImage.Opacity = 1;
                }
                else
                {
                    UpButton.IsEnabled = false;
                    UpImage.Opacity = 0.3;
                }
            }
            if (e.PropertyName.Equals("NavigateBackAvailable"))
            {
                if (((NavigationButtonValues)sender).NavigateBackAvailable)
                {
                    BackButton.IsEnabled = true;
                    BackImage.Opacity = 1;
                }
                else
                {
                    BackButton.IsEnabled = false;
                    BackImage.Opacity = 0.3;
                }
            }
            if (e.PropertyName.Equals("NavigateForwardAvailable"))
            {
                if (((NavigationButtonValues)sender).NavigateForwardAvailable)
                {
                    ForwardButton.IsEnabled = true;
                    ForwardImage.Opacity = 1;
                }
                else
                {
                    ForwardButton.IsEnabled = false;
                    ForwardImage.Opacity = 0.3;
                }
            }
        }

        private void ChangeHistory_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("RedoAvailable"))
            {
                if (((ChangeHistory)sender).RedoAvailable)
                {
                    RedoButton.IsEnabled = true;
                    RedoImage.Opacity = 1;
                }
                else
                {
                    RedoButton.IsEnabled = false;
                    RedoImage.Opacity = 0.3;
                }
            }
            else if (e.PropertyName.Equals("UndoAvailable"))
            {
                if (((ChangeHistory)sender).UndoAvailable)
                {
                    UndoButton.IsEnabled = true;
                    UndoImage.Opacity = 1;
                }
                else
                {
                    UndoButton.IsEnabled = false;
                    UndoImage.Opacity = 0.3;
                }
            }
        }

        private void sfTreeGrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grids.GridSelectionChangedEventArgs e)
        {
            Debug.WriteLine("selection changed!");
            MenuFlyout menuFlyout = new MenuFlyout();
            var newlySelected = ((TreeGridRowInfo)e.AddedItems.FirstOrDefault())?.RowData as ExplorerItem;
            if (newlySelected != null && newlySelected.GetType() == typeof(PCOFolder))
            {
                MenuFlyoutItem setRootItem = new MenuFlyoutItem() { Text = "Navigate to folder" };
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

        private async void ExpandClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Expand clicked");
            ExplorerItem item = ((Button)sender).DataContext as ExplorerItem;
            await DialogHandler.ShowExplorerItemDialog(item, this.XamlRoot);
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Back clicked");
            ViewModel.NavigateBack();
            UpdateGraphViewIfActive();
        }

        private void ForwardClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Forward clicked");
            ViewModel.NavigateForward();
            UpdateGraphViewIfActive();
        }

        private void UpClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Up clicked");
            ViewModel.NavigateUp();
            UpdateGraphViewIfActive();
        }

        private void SetRoot(object sender, RoutedEventArgs e)
        {
            var selectedItem = rootTreeGrid.SelectedItem as ExplorerItem;
            Debug.WriteLine("set new root folder = " + selectedItem.Name);
            if (selectedItem.GetType() == typeof(PCOFolder))
            {
                Debug.WriteLine("this is folder");
                ViewModel.NavigateNewRoot((PCOFolder)selectedItem);
            }
            else
            {
                Debug.WriteLine("this is file");
            }

        }

        private void UpdateGraphViewIfActive()
        {
            if (this.GraphViewActive)
            {
                GraphHolder.Content = GraphHelper.GetCurrentSunburst(ExplorerPageName.Resources["SunburstTooltipTemplate"] as DataTemplate, (PointerEventHandler)SunburstOnClickAsync);
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
                    ViewModel.NavigateToPath(selectedFolder.Path);
                }
            }
            catch (Exception ex)
            {
                await DialogHandler.ShowErrorDialog(ex.Message, this.Content.XamlRoot);
                return;
            }
        }

        private void SfComboBox_SelectionChanged(object sender, Syncfusion.UI.Xaml.Editors.ComboBoxSelectionChangedEventArgs e)
        {
            var item = ((Syncfusion.UI.Xaml.Editors.SfComboBox)sender).DataContext as ExplorerItem;
            var previousOwner = item.SelectedOwner;
            if (e.AddedItems?.Count == 0 || e.AddedItems[0].GetType() == typeof(string) || ((IOwner)e.AddedItems[0]).Name.Equals("Unselected"))
            {
                ((Syncfusion.UI.Xaml.Editors.SfComboBox)sender).SelectedItem = null;
                if(previousOwner != null && !previousOwner.Name.Equals("Unselected"))
                {
                    item.SelectedOwner = null;
                    item.SelectedOwnerColor = null;
                    item.SelectedOwnerName = null;
                    PCOState.GetInstance().ChangeHistory.AddChange(new OwnerChange(previousOwner, null, item, (SfComboBox) sender));
                }
                
                return;
            }
            Debug.WriteLine("Changed selected owner");
            var newOwner = (IOwner)e.AddedItems[0];
            if (!newOwner.Equals(previousOwner))
            {
                item.SelectedOwner = newOwner;
                item.SelectedOwnerColor = null;// TODO: maybe less hacky fix
                item.SelectedOwnerName = null;
                PCOState.GetInstance().ChangeHistory.AddChange(new OwnerChange(previousOwner, newOwner, item, (SfComboBox)sender));
            }
            
        }

        private void GraphExplorerSwitch(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("clicked graph button");
            if (GraphViewActive)
            {
                //go to explorer view
                ViewSwitch.Content = "Graph View";
                GraphViewActive = false;
                GraphView.Visibility = Visibility.Collapsed;
                rootTreeGrid.Visibility = Visibility.Visible;

            }
            else
            {
                //go to graph view
                ViewSwitch.Content = "Explorer View";
                GraphViewActive = true;
                rootTreeGrid.Visibility = Visibility.Collapsed;
                GraphView.Visibility = Visibility.Visible;
                //GraphHolder.Content = GraphHelper.GetCurrentTreeGraph();
                GraphHolder.Content = GraphHelper.GetCurrentSunburst(ExplorerPageName.Resources["SunburstTooltipTemplate"] as DataTemplate, (PointerEventHandler)SunburstOnClickAsync);
            }

        }
        private void SunburstOnClickAsync(object sender, PointerRoutedEventArgs e)
        {
            var source = e.OriginalSource as Path;
            if (source != null)
            {
                var tag = source.Tag as ChartSegment;
                var clickedItem = (dynamic)tag.Item;

                if (clickedItem.ExplorerItem == null) return;
                
                ViewModel.SelectedGraphItem = clickedItem.ExplorerItem;

                if (e.GetCurrentPoint((UIElement)sender).Properties.IsRightButtonPressed)
                {
                    var dc = ((FrameworkElement)e.OriginalSource).DataContext as ExplorerViewModel;

                    MenuFlyout flyout = new MenuFlyout();
                    MenuFlyoutItem flyoutItem = new MenuFlyoutItem();
                    flyoutItem.Text = "Navigate to: " + clickedItem.Name;
                    flyoutItem.Click += async delegate (object sender, RoutedEventArgs e)
                    {
                        if (clickedItem.ExplorerItem.GetType() == typeof(PCOFolder))
                        {
                            ViewModel.NavigateNewRoot((PCOFolder)clickedItem.ExplorerItem);
                            GraphHolder.Content = GraphHelper.GetCurrentSunburst(ExplorerPageName.Resources["SunburstTooltipTemplate"] as DataTemplate, SunburstOnClickAsync);
                        }
                        else
                        {
                            await DialogHandler.ShowErrorDialog("Navigation not possible. Selected item is a file", this.XamlRoot);
                        }
                    };

                    flyout.Items.Add(flyoutItem);

                    Microsoft.UI.Input.PointerPoint pointerPoint = e.GetCurrentPoint((UIElement)sender);
                    Point ptElement = new Point(pointerPoint.Position.X, pointerPoint.Position.Y);
                    flyout.ShowAt((FrameworkElement)sender, ptElement);
                }
            }

        }

        private void UndoClick(object sender, RoutedEventArgs e)
        {
            PCOState.GetInstance().ChangeHistory.Undo();
        }

        private void RedoClick(object sender, RoutedEventArgs e)
        {
            PCOState.GetInstance().ChangeHistory.Redo();
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}

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
using System.ComponentModel;
using Syncfusion.UI.Xaml.Data;
using Windows.Devices.Printers.Extensions;
using Project_Codebase_Overview.LocalSettings;
using System.Threading;

namespace Project_Codebase_Overview
{
    public sealed partial class ExplorerPage : Page
    {
        ExplorerViewModel ViewModel;

        public delegate void NotifyInitialStart();
        public event NotifyInitialStart NotifyInitialStartEvent;


        bool GraphViewActive = false;
        public ExplorerPage()
        {
            this.InitializeComponent();

            ViewModel = (ExplorerViewModel)this.DataContext;

            var root = PCOState.GetInstance().GetExplorerState().GetCurrentRootFolder();

            ViewModel.SetExplorerItems(root);
            ViewModel.SelectedGraphItem = root;

            rootTreeGrid.SelectionChanged += sfTreeGrid_SelectionChanged;

            PCOState.GetInstance().ChangeHistory.PropertyChanged += ChangeHistory_PropertyChanged;
            this.UpdateUndoRedoButtons();

            ViewModel.navButtonValues.PropertyChanged += NavButtonPropertyChanged;

            if (!PCOState.GetInstance().GetSettingsState().IsDecayActive)
            {
                rootTreeGrid.Columns.RemoveAt(4); //Remove decay column
            }

            ViewModel.UpdateBreadcrumbBar();

            rootTreeGrid.SortColumnsChanged += RootTreeGrid_SortColumnsChanged;

            if (PCOState.GetInstance().GetExplorerState().GraphViewActive)
            {
                GraphExplorerSwitch(null, null);
            }

            ViewModel.NotifyGraphUpdateEvent += UpdateGraphViewIfActive;

            PCOState.GetInstance().GetExplorerState().NotifyChangeEvent += NotifyExplorerUpdate;

            NotifyInitialStartEvent += NotifyInitialStartFunc;

            if (!LocalSettingsHelper.GetIsInitialTutorialShown())
            {
                var queue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    queue.TryEnqueue(() =>
                    {
                        NotifyInitialStartEvent?.Invoke();
                    });
                });
            }
        }

        private async void NotifyInitialStartFunc()
        {
            var result = await DialogHandler.ShowYesNoDialog(XamlRoot, "Welcome to PCO!", "Would you like to view the tutorial?");
            if (result)
            {
                await DialogHandler.ShowHelpDialog(XamlRoot);
            }
            LocalSettingsHelper.SetIsInitialTutorialShown();
        }

        private async void NotifyExplorerUpdate()
        {
            if (!LocalSettingsHelper.GetIsExplorerUpdateExplained())
            {
                LocalSettingsHelper.SetIsExplorerUpdateExplained();
                await DialogHandler.ShowOkDialog("Explorer updater", "You just updated an explorer item!\n\nTo view the changes this has caused, click the \"Update explorer\"-button in the settings-panel", XamlRoot);
            }
        }

        public ObservableCollection<IOwner> Owners { get => this.GetOwnerListSorted(); }
        private ObservableCollection<IOwner> GetOwnerListSorted()
        {
            //create "Unselected" entry
            var ownerlist = PCOState.GetInstance().GetContributorState().GetAllOwnersInMode().OrderBy(x => x.Name).ToList();
            ownerlist.Add(new Author("None", "None"));
            return ownerlist.ToObservableCollection();
        }

        private void RootTreeGrid_SortColumnsChanged(object sender, Syncfusion.UI.Xaml.Grids.GridSortColumnsChangedEventArgs e)
        {
            var comparer = rootTreeGrid.SortComparers.Where(c => c.PropertyName.Equals("Name")).Single().Comparer as CustomSortNameComparer;
            comparer.SortDirection = e.AddedItems.Where(i => i.ColumnName.Equals("Name")).FirstOrDefault()?.SortDirection ?? comparer.SortDirection;
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

        private void UpdateUndoRedoButtons()
        {
            if (PCOState.GetInstance().ChangeHistory.RedoAvailable)
            {
                RedoButton.IsEnabled = true;
                RedoImage.Opacity = 1;
            }
            else
            {
                RedoButton.IsEnabled = false;
                RedoImage.Opacity = 0.3;
            }
            if (PCOState.GetInstance().ChangeHistory.UndoAvailable)
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

        private void ChangeHistory_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("RedoAvailable"))
            {
                this.UpdateUndoRedoButtons();
            }
            else if (e.PropertyName.Equals("UndoAvailable"))
            {
                this.UpdateUndoRedoButtons();  
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

                MenuFlyoutItem openFolderItem = new MenuFlyoutItem() { Text = "Open in File explorer" };
                openFolderItem.Click += this.OpenFileExplorer;
                menuFlyout.Items.Add(openFolderItem);
            }
            else
            {
                MenuFlyoutItem openFolderItem = new MenuFlyoutItem() { Text = "Open location in File explorer" };
                openFolderItem.Click += this.OpenFileExplorer;
                menuFlyout.Items.Add(openFolderItem);
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

        private async void OpenFileExplorer(object sender, RoutedEventArgs e)
        {
            ExplorerItem selectedItem;
            if (GraphViewActive)
            {
                selectedItem = ((ExplorerViewModel)((MenuFlyoutItem)sender).DataContext).SelectedGraphItem as ExplorerItem;
            } else
            {
                selectedItem = rootTreeGrid.SelectedItem as ExplorerItem;
            }

            PCOFolder foundFolder;
            if (selectedItem.GetType() == typeof(PCOFolder))
            {
                foundFolder = (PCOFolder)selectedItem;
            } else
            {
                foundFolder = selectedItem.Parent as PCOFolder;
            }
            var path = PCOState.GetInstance().GetExplorerState().GetRootPath() + "\\" + foundFolder.GetRelativePath();
            try
            {
                if (Directory.Exists(path))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        Arguments = path,
                        FileName = "explorer.exe"
                    };

                    Process.Start(startInfo);

                }
                else
                {
                    await DialogHandler.ShowErrorDialog("Could not find the path \"" + path + "\"", XamlRoot);
                }
            }
            catch
            {
                await DialogHandler.ShowErrorDialog("Could not access \"" + path + "\"", XamlRoot);
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
            folderPicker.FileTypeFilter.Add("*"); // work around to fix crashing of packaged app
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
                if (PCOState.GetInstance().GetExplorerState().GraphViewHasChanges)
                {
                    PCOState.GetInstance().GetExplorerState().GraphViewActive = GraphViewActive;
                    PCOState.GetInstance().GetExplorerState().GraphViewHasChanges = false;
                    PCOState.GetInstance().GetExplorerState().ReloadExplorer();
                }

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
            PCOState.GetInstance().GetExplorerState().GraphViewActive = GraphViewActive;
            PCOState.GetInstance().GetExplorerState().GraphViewHasChanges = false;

        }
        private void SunburstOnClickAsync(object sender, PointerRoutedEventArgs e)
        {
            var source = e.OriginalSource as Path;
            if (source != null)
            {
                var tag = source.Tag as ChartSegment;
                var clickedItem = (dynamic)tag.Item;

                if (clickedItem.ExplorerItem == null || (!PCOState.GetInstance().GetSettingsState().IsFilesVisibile && clickedItem.ExplorerItem.GetType() == typeof(PCOFile))) return;
                
                ViewModel.SelectedGraphItem = clickedItem.ExplorerItem;

                if (e.GetCurrentPoint((UIElement)sender).Properties.IsRightButtonPressed)
                {
                    var dc = ((FrameworkElement)e.OriginalSource).DataContext as ExplorerViewModel;

                    MenuFlyout flyout = new MenuFlyout();
                    if (dc.SelectedGraphItem.GetType() == typeof(PCOFolder))
                    {
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

                        MenuFlyoutItem openFolderItem = new MenuFlyoutItem() { Text = "Open in File explorer" };
                        openFolderItem.Click += this.OpenFileExplorer;
                        flyout.Items.Add(openFolderItem);
                    } 
                    else
                    {

                        MenuFlyoutItem openFolderItem = new MenuFlyoutItem() { Text = "Open location in File explorer" };
                        openFolderItem.Click += this.OpenFileExplorer;
                        flyout.Items.Add(openFolderItem);
                    }

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

        private void SfComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            return;
            var comboBox = (Syncfusion.UI.Xaml.Editors.SfComboBox)sender;
            var item = comboBox.DataContext as ExplorerItem;

            comboBox.SelectedItems.Clear();
            foreach (var owner in item.SelectedOwners.Select(x => x).ToArray())
            {
                comboBox.SelectedItems.Add(owner);
            }

        }
    }

    public class CustomSortNameComparer : IComparer<object>
    {

        public int Compare(object x, object y)
        {
            var item1 = x as ExplorerItem;
            var item2 = y as ExplorerItem;

            var value1 = item1.Name;
            var value2 = item2.Name;
            int c = 0;

            if (item1.GetType() == typeof(PCOFolder) && item2.GetType() == typeof(PCOFile))
            {
                return SortDirection == SortDirection.Descending ? 1 : -1;
            }

            if (item1.GetType() == typeof(PCOFile) && item2.GetType() == typeof(PCOFolder))
            {
                return SortDirection == SortDirection.Descending ? -1 : 1;
            }


            if (value1 == null && value2 != null)
            {
                c = -1;
            }
            


            else if (value1 != null && value2 == null)
            {
                c = 1;
            }

            else if (value1 != null && value2 != null)
            {
                c = string.Compare(value1, value2, StringComparison.InvariantCulture);
            }



            return c;
        }

        //Get or Set the SortDirection value
        private SortDirection _SortDirection;

        public SortDirection SortDirection
        {
            get { return _SortDirection; }
            set { _SortDirection = value; }
        }
    }
    public class CustomCellTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {

            if (item == null)
                return null;

            var data = item as ExplorerItem;

            if (data.GetType() == typeof(PCOFile))
                return App.Current.Resources["NameTemplateFile"] as DataTemplate;

            else
                return App.Current.Resources["NameTemplateFolder"] as DataTemplate;
        }
    }
}

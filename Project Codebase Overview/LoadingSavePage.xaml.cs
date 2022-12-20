using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.DataCollection.Model;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using System.Threading;
using static System.Net.WebRequestMethods;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    public class LoadingSavePageParameters
    {
        public StorageFile File;
        public LoadingSavePageParameters(StorageFile file)
        {
            File = file;
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoadingSavePage : Page
    {
        string FileName;
        StorageFile File;
        public LoadingSavePage()
        {
            this.InitializeComponent();
            this.Loaded += LoadFile;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadingSavePageParameters parameters = e.Parameter as LoadingSavePageParameters;
            File = parameters.File;
            FileName = File.Name;
        }

        private async void LoadFile(object sender, RoutedEventArgs e)
        {
            
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            var state = PCOState.GetInstance();
            state.ClearState();
            await System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    Thread.Sleep(3000);
                    bool repoChangesAvailable = await state.LoadFile(File);
                    dispatcherQueue?.TryEnqueue(async () =>
                    {
                        if (repoChangesAvailable)
                        {
                            bool loadNewData = await DialogHandler.ShowYesNoDialog(Content.XamlRoot, "Load",
                                "The saved state is deprecated. Changes have been made since last opened. Do you want to load the changes?");
                            if (loadNewData)
                            {
                                PCOState.GetInstance().GetLoadingState().IsLoadingNewState = false;
                                //goto loading page
                                ((Application.Current as App)?.MainWindow as MainWindow).NavigateToLoadingPage();
                            }
                            else
                            {
                                ((Application.Current as App)?.MainWindow as MainWindow).NavigateToExplorerPage();
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                            var window = ((Application.Current as App)?.MainWindow as MainWindow);
                            ((Application.Current as App)?.MainWindow as MainWindow).NavigateToExplorerPage();
                        }
                    });
                   
                }
                catch (Exception ex)
                {
                    dispatcherQueue?.TryEnqueue(async () =>
                    {
                        await DialogHandler.ShowErrorDialog(ex.Message, Content.XamlRoot);

                        var currentWindow = (Application.Current as App)?.MainWindow as MainWindow;

                        //return to home :) 
                        
                        PCOState.GetInstance().ClearState();
                        StartWindow window = new();
                        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
                        OverlappedPresenter presenter = appWindow.Presenter as OverlappedPresenter;

                        appWindow.Resize(new Windows.Graphics.SizeInt32(900, 600));
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

                        currentWindow.Close();
                        
                        
                    });
                    
                }
                
            });

            
        }
    }
}

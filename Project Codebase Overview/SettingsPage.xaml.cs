using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.Settings;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Editors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Globalization.NumberFormatting;
using Project_Codebase_Overview.Dialogs;
using Project_Codebase_Overview.DataCollection;
using Project_Codebase_Overview.FileExplorerView;
using Project_Codebase_Overview.SaveState.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        ObservableCollection<IOwner> OwnersList = new ObservableCollection<IOwner>();
        private bool IsExpanded = true;
        private bool InitialOpenDone = false;


        public SettingsPage()
        {
            this.InitializeComponent();


            DecayCheckBox.IsChecked = true;

            UpdateOwnerList();

            ExpanderClick(null,null);

            LoadSettingsFromState();
        }

        private void UpdateOwnerList()
        {
            OwnersList.Clear();
            var list = PCOState.GetInstance().GetContributorState().GetAllOwners();
            foreach (var owner in list)
            {
                OwnersList.Add(owner);
            }
        }

        private void LoadSettingsFromState()
        {
            var settingsState = PCOState.GetInstance().GetSettingsState();

            CutOffSelectionComboBox.SelectedIndex = ((int)settingsState.CutOffSelectionUnit)-1;

            BranchNameBlock.Text = PCOState.GetInstance().GetBranchName();

            DecayCheckBox.IsChecked = settingsState.IsDecayActive;

            DecayTimerNumberBox.Value = settingsState.DecayDropOffInteval;
            if (settingsState.DecayTimeUnit != DecayTimeUnit.UNDEFINED)
            {
                DecayTimerComboBox.SelectedIndex = ((int)settingsState.DecayTimeUnit)-1;
            } else
            {
                DecayTimerComboBox.SelectedItem = null;
            }
            PercentageNumberBox.Value = settingsState.DecayPercentage;

            ShowFilesCheckBox.IsChecked = settingsState.IsFilesVisibile;

        }

        private void CancelSettingsChangeClick(object sender, RoutedEventArgs e)
        {
            LoadSettingsFromState();
        }

        private void SaveSettingsChangeClick(object sender, RoutedEventArgs e)
        {
            var settingsState = PCOState.GetInstance().GetSettingsState();

            var selectedCutOff = CutOffSelectionComboBox.SelectedItem != null ? CutOffSelectionComboBox.SelectedIndex + 1 : 1;
            settingsState.CutOffSelectionUnit = (CutOffSelectionUnit)selectedCutOff;

            settingsState.IsDecayActive = DecayCheckBox.IsChecked ?? false;

            settingsState.DecayDropOffInteval = ((int?)DecayTimerNumberBox.Value) ?? 0;

            var selected = DecayTimerComboBox.SelectedItem != null ? DecayTimerComboBox.SelectedIndex + 1 : 1;
            settingsState.DecayTimeUnit = (DecayTimeUnit) selected;

            settingsState.DecayPercentage = ((int?)PercentageNumberBox.Value) ?? 0;

            settingsState.IsFilesVisibile = ShowFilesCheckBox.IsChecked ?? true;

            LoadSettingsFromState();

            PCOState.GetInstance().GetExplorerState().ReloadExplorer();
        }

        private void ManageClicked(object sender, RoutedEventArgs e)
        {
            ((Application.Current as App)?.MainWindow as MainWindow).NavigateToManagementPage();
        }

        private void DecayChecked(object sender, RoutedEventArgs e)
        {
            DecayCheckBox.Content = "Enabled";
            DecayTimerNumberBox.IsEnabled = true;
            DecayTimerComboBox.IsEnabled = true;
            PercentageNumberBox.IsEnabled = true;
        }

        private void DecayUnchecked(object sender, RoutedEventArgs e)
        {
            DecayCheckBox.Content = "Disabled";
            DecayTimerNumberBox.IsEnabled = false;
            DecayTimerComboBox.IsEnabled = false;
            PercentageNumberBox.IsEnabled = false;
        }

        private void ShowFilesChecked(object sender, RoutedEventArgs e)
        {
            ShowFilesCheckBox.Content = "Enabled";
        }

        private void ShowFilesUnchecked(object sender, RoutedEventArgs e)
        {
            ShowFilesCheckBox.Content = "Disabled";
        }

        private void ExpanderClick(object sender, PointerRoutedEventArgs e)
        {
            if (IsExpanded)
            {
                IsExpanded = false;
                SettingsPanelGrid.Visibility = Visibility.Collapsed;
                ExpanderText1.Text = ">";
                ExpanderText2.Text = ">";
                ExpanderText3.Text = ">";
            }
            else
            {
                IsExpanded = true;
                SettingsPanelGrid.Visibility = Visibility.Visible;
                ExpanderText1.Text = "<";
                ExpanderText2.Text = "<";
                ExpanderText3.Text = "<";
            }
        }

        private void OwnerModeChanged(object sender, Syncfusion.UI.Xaml.Editors.SegmentSelectionChangedEventArgs e)
        {
            if (!this.InitialOpenDone)
            {
                this.InitialOpenDone = true;
                return;
            }
            if (e.NewValue.Equals("Users"))
            {
                PCOState.GetInstance().GetSettingsState().CurrentMode = PCOExplorerMode.USER;
            }
            else if (e.NewValue.Equals("Teams"))
            {
                PCOState.GetInstance().GetSettingsState().CurrentMode = PCOExplorerMode.TEAMS;
            }
            //Update settingspanel owner list
            UpdateOwnerList();
            //Reload explorerview
            PCOState.GetInstance().GetExplorerState().ReloadExplorer();
        }

        private async void SaveClick(object sender, RoutedEventArgs e)
        {
            
            FileSavePicker savePicker = new FileSavePicker();
            IntPtr windowHandler = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.MainWindow as MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandler);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Clear();
            savePicker.FileTypeChoices.Add("json", new List<string>() { ".json" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Project State - " + DateTime.Now.Date.ToString();

            StorageFile file = await savePicker.PickSaveFileAsync();
            string outputText = "";
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                await PCOState.GetInstance().SaveStateToFile(file);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    outputText = "Saved file: " + file.Name;
                }
                else
                {
                    outputText = "An error occurred While saving file: " + file.Name;
                }
            }
            else
            {
                outputText = "Save cancelled.";
            }
            Debug.WriteLine(outputText);
            ((Application.Current as App)?.MainWindow as MainWindow).ShowToast(outputText);
        }

        private async void LoadClick(object sender, RoutedEventArgs e)
        {
            //TODO: Save changes before exit!?!
            
            PCOState.GetInstance().ClearState();

            var filePicker = new FileOpenPicker();
            IntPtr windowHandler = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.MainWindow as MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, windowHandler);
            filePicker.FileTypeFilter.Add(".json");
            var file = await filePicker.PickSingleFileAsync();

            if(file == null)
            {
                return;
            }

            bool repoChangesAvailable = await PCOState.GetInstance().LoadFile(file);

            ((Application.Current as App)?.MainWindow as MainWindow).ShowToast("Loaded file: " + file.Name);

            if (repoChangesAvailable)
            {
                bool loadNewData = await DialogHandler.ShowYesNoDialog(XamlRoot, "Load", 
                    "The saved state is deprecated. Changes have been made since last opened. Do you want to load the changes?");
                if (loadNewData)
                {
                    PCOState.GetInstance().GetLoadingState().IsLoadingNewState = false;
                    //goto loading page
                    ((Application.Current as App)?.MainWindow as MainWindow).NavigateToLoadingPage();

                }
            }
           

            
        }

        private async void NewRepoClick(object sender, RoutedEventArgs e)
        {

        }
    }
}

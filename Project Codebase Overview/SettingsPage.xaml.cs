using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.State;
using Syncfusion.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
        ObservableCollection<string> DataSelectionOptions = new ObservableCollection<string>();
        private bool IsExpanded = true;
        private bool InitialOpenDone = false;


        public SettingsPage()
        {
            this.InitializeComponent();

            DataSelectionOptions.Add("All time");
            DataSelectionOptions.Add("6 months");
            DataSelectionOptions.Add("1 year");
            DataSelectionOptions.Add("2 years");
            DataSelectionOptions.Add("3 years");
            DataSelectionOptions.Add("5 years");

            DecayCheckBox.IsChecked = true;

            UpdateOwnerList();

            ExpanderClick(null,null);
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

        private void ManageClicked(object sender, RoutedEventArgs e)
        {
            ((Application.Current as App)?.MainWindow as MainWindow).NavigateToManagementPage();
        }

        private void DecayChecked(object sender, RoutedEventArgs e)
        {
            DecayCheckBox.Content = "Enabled";
            DecayTimerNumberBox.Visibility = Visibility.Visible;
            DecayTimerComboBox.Visibility = Visibility.Visible;
            DecayTimerTextBlock.Visibility = Visibility.Visible;
            DropOffNumberBox.Visibility = Visibility.Visible;
            DropOffTextBlock.Visibility = Visibility.Visible;
        }

        private void DecayUnchecked(object sender, RoutedEventArgs e)
        {
            DecayCheckBox.Content = "Disabled";
            DecayTimerNumberBox.Visibility = Visibility.Collapsed;
            DecayTimerComboBox.Visibility = Visibility.Collapsed;
            DecayTimerTextBlock.Visibility = Visibility.Collapsed;
            DropOffNumberBox.Visibility = Visibility.Collapsed;
            DropOffTextBlock.Visibility = Visibility.Collapsed;
        }

        private void ShowFilesChecked(object sender, RoutedEventArgs e)
        {
            ShowFilesCheckBox.Content = "Enabled";
        }

        private void ShowFilesUnchecked(object sender, RoutedEventArgs e)
        {
            ShowFilesCheckBox.Content = "Disabled";
        }

        private void DecayTimerNumberChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {

        }

        private void DecayTimerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DecayDropOffChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {

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
                PCOState.GetInstance().GetSettingsState().CurrentMode = Mode.USER;
            }
            else if (e.NewValue.Equals("Teams"))
            {
                PCOState.GetInstance().GetSettingsState().CurrentMode = Mode.TEAMS;
            }
            //Update settingspanel owner list
            UpdateOwnerList();
            //Reload explorerview
            PCOState.GetInstance().GetExplorerState().ReloadExplorer();
        }
    }
}

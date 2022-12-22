using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Codebase_Overview.DataCollection.Model;
using Syncfusion.UI.Xaml.Charts;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Project_Codebase_Overview.Graphs;
using Project_Codebase_Overview.Graphs.Model;
using System.Diagnostics;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.State;

namespace Project_Codebase_Overview.Dialogs
{
    public static class DialogHandler
    {
        public static async Task<ContentDialogResult> ShowErrorDialog(string errorText, XamlRoot xamlRoot)
        {
            TextBlock contentText = new TextBlock();
            contentText.Text = errorText;
            contentText.TextWrapping = TextWrapping.WrapWholeWords;

            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = xamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Error";
            dialog.PrimaryButtonText = "Ok";
           
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = contentText;

            var result = await dialog.ShowAsync();
            return result;
        }
        public static async Task<ContentDialogResult> ShowOkDialog(string title, string text, XamlRoot xamlRoot)
        {
            TextBlock contentText = new TextBlock();
            contentText.Text = text;

            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = xamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = "Ok";

            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = contentText;

            var result = await dialog.ShowAsync();
            return result;
        }

        public static async Task<ContentDialogResult> ShowExplorerItemDialog(ExplorerItem item, XamlRoot xamlRoot)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = xamlRoot;
            
            ExpandDialogParameters parameters = new ExpandDialogParameters();
            parameters.Item = item;
            parameters.Dialog = dialog;

            var frame = new Frame();
            frame.Navigate(typeof(ExpandExplorerItemDialog), parameters);
            dialog.Content = frame;


            var result = await dialog.ShowAsync();
            return result;
        }

        public static async Task<ContentDialogResult> ShowAddEditTeamDialog(XamlRoot xamlRoot, PCOTeam team = null)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = xamlRoot;

            var manager = PCOState.GetInstance().GetContributorState();
            if (team == null)
            {
                // new team
                dialog.Title = "New Team";
                manager.SetSelectedTeam(null);
            } else
            {
                // edit team
                dialog.Title = "Edit Team";
                manager.SetSelectedTeam(team);
            }


            manager.SetCurrentTeamDialog(dialog);

            var frame = new Frame();
            frame.Navigate(typeof(AddEditTeamDialogPage));
            dialog.Content = frame;
            
            
            var result = await dialog.ShowAsync();
            return result;
        }

        public static async Task<ContentDialogResult> ShowEditAuthorDialog(XamlRoot xamlRoot, Author author)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = xamlRoot;

            var manager = PCOState.GetInstance().GetContributorState();
            dialog.Title = "Edit Author";

            manager.SetSelectedAuthor(author);
            manager.SetCurrentAuthorDialog(dialog);

            var frame = new Frame();
            frame.Navigate(typeof(EditAuthorDialogPage));
            dialog.Content = frame;
            
            
            var result = await dialog.ShowAsync();
            return result;
        }

        private static void LineDistSeries_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> ShowYesNoDialog(XamlRoot xamlRoot, string title, string description)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = xamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = "Yes";
            dialog.SecondaryButtonText = "No";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new TextBlock() { Text = description, TextWrapping = TextWrapping.WrapWholeWords};

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

    }
}

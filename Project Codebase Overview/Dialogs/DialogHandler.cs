using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.Dialogs
{
    public static class DialogHandler
    {
        public static async Task<ContentDialogResult> ShowErrorDialog(string errorText, XamlRoot xamlRoot)
        {
            TextBlock contentText = new TextBlock();
            contentText.Text = errorText;

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
    }
}

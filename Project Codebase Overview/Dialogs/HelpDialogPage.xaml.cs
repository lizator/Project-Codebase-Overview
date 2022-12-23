using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview.Dialogs
{
    public class HelpDialogElement : ObservableObject
    {
        private string _source;
        public string Source { get => _source; set => SetProperty(ref _source, value); }
        private string _description;
        public string Description { get => _description; set => SetProperty(ref _description, value); }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HelpDialogPage : Page
    {
        private List<HelpDialogElement> Elements = new List<HelpDialogElement>();
        private int CurrentIndex = 0;
        public  HelpDialogElement CurrentElement { get; set; }
       
        public HelpDialogPage()
        {
            this.InitializeComponent();

            //load all the images
            string dir = "../Assets/";
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Monkey.png",
                Description = "This is a beatuiful monkey \n I likes all the bananas and so on :))))"
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Explorer.png",
                Description = "This is the explorer. Here you will see the data for the entire git repository.\n" +
                "Hover your mouse over the headers to see information about each column."
            });


            //set current element
            CurrentElement = Elements[0];
        }

        private void PreviousClick(object sender, RoutedEventArgs e)
        {
            if(CurrentIndex > 0)
            {
                CurrentIndex--;
                CurrentElement = Elements[CurrentIndex];
            }
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < Elements.Count() - 1)
            {
                CurrentIndex++;
                CurrentElement = Elements[CurrentIndex];
            }
        }
    }
}

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
using Windows.Networking.NetworkOperators;

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
                Source = dir + "Explorer.png",
                Description = "This is the explorer. Here you will see the data for the entire git repository.\n" +
                "This is the main view, where you will navigate the file structure to select owners for files and folders.\n" +
                "Hover your mouse over the headers in the grid to see information about each column."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Explorer1.png",
                Description = "Use these arrows to expand or collapse folders to toggle showing their content."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Explorer2.png",
                Description = "You can right click a folder and navigate to it to only show the contents of that folder."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Explorer5.png",
                Description = "For more details about a file or folder and access to comments you can click the expand buttons."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Explorer6.png",
                Description = "If you want a graphical representation of the currently navigated folder, click the graph overview button."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Explorer3.png",
                Description = "You can select owners for folders and files in this drop down.\nYou can select both teams and authors at the same time."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Settings1.png",
                Description = "Open up the settings panel by clicking the bar on the left side of the window."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Settings2.png",
                Description = "From here you can change the settings of the explorer, access team management, save state etc."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Settings3.png",
                Description = "After changing selected owners in the explorer click this update button to update the line distribution bar graphs of the explorer."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Explorer4.png",
                Description = "Here you see that after selecting Bob as owner for a subfolder and updating, the suggested owner and the graph changes for the parent folder."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Management1.png",
                Description = "To begin setting up teams and authors go the management page by clicking here."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Management2.png",
                Description = "This is the management page. You can switch between viewing and editing authors or teams with the tabs in the top left corner.\n" +
                "Click the button marked with red to create a team."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "Save.png",
                Description = "After creating teams and selecting owners remember to save the state using this button.\n" +
                "Saved states load much faster than opening a new repository and scanning it again."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "CodeOwners.png",
                Description = "You can also export a CodeOwners file with the ownership you have set in the program.\n" +
                "The file is then ready to use in GitHub, GitLab etc."
            });
            Elements.Add(new HelpDialogElement()
            {
                Source = dir + "CodeOwners1.png",
                Description = "This is an example of a CodeOwners file from two selected owners."
            });
            //set current element
            CurrentElement = new HelpDialogElement() ;
            SetCurrentElement();
        }

        private void PreviousClick(object sender, RoutedEventArgs e)
        {
            if(CurrentIndex > 0)
            {
                CurrentIndex--;
                SetCurrentElement();
                NextButton.IsEnabled = true;
            }
            if(CurrentIndex == 0)
            {
                PrevButton.IsEnabled = false;
            }
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            if (CurrentIndex < Elements.Count() - 1)
            {
                CurrentIndex++;
                SetCurrentElement();
                PrevButton.IsEnabled = true;
            }
            if(CurrentIndex == Elements.Count - 1)
            {
                NextButton.IsEnabled = false;
            }
        }
        private void SetCurrentElement()
        {
            CurrentElement.Source = Elements[CurrentIndex].Source;
            CurrentElement.Description = Elements[CurrentIndex].Description;
        }

        private void PipChangedIndex(PipsPager sender, PipsPagerSelectedIndexChangedEventArgs args)
        {
            SetCurrentElement();
        }
    }
}

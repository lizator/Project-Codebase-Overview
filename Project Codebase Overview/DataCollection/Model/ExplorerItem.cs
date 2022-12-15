using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Project_Codebase_Overview.ContributorManagement;
using Project_Codebase_Overview.ContributorManagement.Model;
using Project_Codebase_Overview.Graphs;
using Syncfusion.UI.Xaml.Data;
using Project_Codebase_Overview.Graphs.Model;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Project_Codebase_Overview.State;
using System.Globalization;
using Syncfusion.UI.Xaml.Editors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Diagnostics;
using Microsoft.UI.Xaml.Data;
using Project_Codebase_Overview.ChangeHistoryFolder;

namespace Project_Codebase_Overview.DataCollection.Model
{
    public abstract class ExplorerItem : ObservableObject, IComparable
    {
        public string Name { get; set; }
        public bool IsActive { get => _isActive; set => SetIsActive(value); }
        private bool _isActive = true;

        private void SetIsActive(bool value)
        {
            SetProperty(ref _isActive, value);
            SelectOwnerVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            InactiveVisibility = !value ? Visibility.Visible : Visibility.Collapsed;
            PCOState.GetInstance().GetExplorerState().ExplorerNotifyChange();
        }

        public Visibility SelectOwnerVisibility { get => _selectOwnerVisibility; set => SetProperty(ref _selectOwnerVisibility, value); }
        private Visibility _selectOwnerVisibility = Visibility.Visible;
        public Visibility InactiveVisibility { get => _inactiveVisibility; set => SetProperty(ref _inactiveVisibility, value); }
        private Visibility _inactiveVisibility = Visibility.Collapsed;
        public uint LinesTotalNumber { get => this.GraphModel.LinesTotal; }
        public uint LinesAfterDecayNumber { get => this.GraphModel.LinesAfterDecay; }
        public abstract void CalculateData();
        public abstract int CompareTo(object obj);
        public abstract string ToCodeowners();
        protected string GetCodeownerLines()
        {
            var builder = new StringBuilder();
            if (this.SelectedOwners.Count > 0 || !this.IsActive)
            {
                builder.AppendLine("");
                if (this.Comment != null && this.Comment.Length > 0)
                {
                    builder.AppendLine("# Comment:");
                    string[] lines = this.Comment.Split(
                        new string[] { Environment.NewLine },
                        StringSplitOptions.None
                    );
                    foreach (var line in lines)
                    {
                        builder.AppendLine("# " + line);
                    }
                }

                var path = "/" + this.GetRelativePath(true);
                path = path.Replace(" ", "\\ ");

                var msg = path;
                if (this.IsActive)
                {
                    var hasError = false;
                    foreach (var owner in this.SelectedOwners)
                    {
                        if (owner.GetType() == typeof(Author))
                        {
                            var vCSEmail = ((Author)owner).VCSEmail;
                            if (vCSEmail != null && vCSEmail.Length > 0)
                            {
                                msg += " " + vCSEmail;
                            }
                            else
                            {
                                builder.AppendLine("# Path \"" + path + "\" should be owned by user \"" + owner.Name + "\" but it did not have an Email set.");
                                PCOState.GetInstance().SetCodeOwnersExportAuthorMissingEmail(true);
                                hasError = true;
                                break;
                            }
                        }
                        else
                        {
                            var vCSID = ((PCOTeam)owner).VCSID;
                            if (vCSID != null && vCSID.Length > 0)
                            {
                                msg += " " + vCSID;
                            }
                            else
                            {
                                builder.AppendLine("# Path \"" + path + "\" should be owned by team \"" + owner.Name + "\" but it did not have an ID set.");
                                PCOState.GetInstance().SetCodeOwnersExportTeamMissingID(true);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    if (!hasError)
                    {
                        builder.AppendLine(msg);
                    }
                } 
                else
                {
                    // Inactive item gets the path in CODEOWNERS without any author or team
                    builder.AppendLine(msg);
                }
                
            }
            return builder.ToString();
        }

        public GraphModel GraphModel { get; set; }
        public PCOFolder Parent { get; set; }

        public string Comment { get => _comment; set => SetProperty(ref _comment, value); }
        private string _comment;

        public string SuggestedOwnerName { get => this.GraphModel.SuggestedOwner?.Name ?? "Undefined"; }
        public SolidColorBrush SuggestedOwnerColor { get => new SolidColorBrush(this.GraphModel.SuggestedOwner?.Color ?? PCOColorPicker.Tranparent); }

        public string SelectedOwnerName { get => this.SelectedOwners.Count == 0 ? "Unselected" : ""; set => SetProperty(ref selectedOwnerName, this.SelectedOwners.Count == 0 ? "Unselected" : ""); }
        private string selectedOwnerName;

        public ObservableCollection<IOwner> SelectedOwners = new ObservableCollection<IOwner>();
        public SfLinearGauge BarGraph
        {
            get => GetBarGraph();
        }
        public SfComboBox SelectOwnerComboBox
        {
            get => GetSelectOwnerComboBox();
        }

        private SfComboBox GetSelectOwnerComboBox()
        {
            var viewSource = new CollectionViewSource();
            viewSource.IsSourceGrouped = true;
            viewSource.Source = Owners;

            var box = new SfComboBox();

            foreach (var owner in SelectedOwners)
            {
                box.SelectedItems.Add(owner);
            }

            box.TokenItemTemplate = Application.Current.Resources["OwnerTokenTemplate"] as DataTemplate;
            box.ItemsSource = viewSource.View;
            box.SelectionChanged += SfComboBox_SelectionChanged;
            box.HorizontalAlignment = HorizontalAlignment.Stretch;
            box.TextMemberPath = "Name";
            box.IsTextSearchEnabled = true;
            box.IsEditable = true;
            box.IsFilteringEnabled = true;
            box.SelectionMode = ComboBoxSelectionMode.Multiple;
            box.MultiSelectionDisplayMode = ComboBoxMultiSelectionDisplayMode.Token;
            box.PlaceholderText = this.SelectedOwners.Count > 0 ? "" : "Unselected";
            box.ItemTemplate = Application.Current.Resources["OwnerItemTemplate"] as DataTemplate;
            box.GroupStyle.Add(Application.Current.Resources["OwnerGroupHeader"] as GroupStyle);


            return box;
        }

        private void SfComboBox_SelectionChanged(object sender, Syncfusion.UI.Xaml.Editors.ComboBoxSelectionChangedEventArgs e)
        {
            var box = (Syncfusion.UI.Xaml.Editors.SfComboBox)sender;
            var item = box.DataContext as ExplorerItem;
            var change = false;


            foreach (var newOwner in e.AddedItems)
            {
                if (newOwner != null && (newOwner.GetType() == typeof(Author) || newOwner.GetType() == typeof(PCOTeam)))
                {
                    if (!item.SelectedOwners.Contains(newOwner))
                    {
                        item.SelectedOwners.Add((IOwner)newOwner);
                        PCOState.GetInstance().ChangeHistory.AddChange(new OwnerChange(null, (IOwner)newOwner, item, (SfComboBox)sender));
                        PCOState.GetInstance().GetExplorerState().GraphViewHasChanges = true;
                        change = true;
                    }
                }
            }
            foreach (var removedOwner in e.RemovedItems)
            {

                if (removedOwner != null && (removedOwner.GetType() == typeof(Author) || removedOwner.GetType() == typeof(PCOTeam)))
                {
                    if (item.SelectedOwners.Contains(removedOwner))
                    {
                        item.SelectedOwners.Remove((IOwner)removedOwner);
                        PCOState.GetInstance().ChangeHistory.AddChange(new OwnerChange((IOwner)removedOwner, null, item, (SfComboBox)sender));
                        PCOState.GetInstance().GetExplorerState().GraphViewHasChanges = true;
                        change = true;
                    }
                }
            }

            box.PlaceholderText = this.SelectedOwners.Count > 0 ? "" : "Unselected";
            item.SelectedOwnerName = null;

            if (change)
            {
                Debug.WriteLine("Changed selected owners");
                PCOState.GetInstance().GetExplorerState().ExplorerNotifyChange();
            }
        }

        public IOrderedEnumerable<IGrouping<string, IOwner>> Owners { get => this.GetOwnerListSorted(); }
        private IOrderedEnumerable<IGrouping<string,IOwner>> GetOwnerListSorted()
        {
            
            //create "Unselected" entry
            var ownerlist = PCOState.GetInstance().GetContributorState().GetAllOwners().OrderBy(x => x.Name).GroupBy(x => x.GetType() == typeof(Author) ? "Authors" : "Teams").OrderBy(g => g.Key);

            if (PCOState.GetInstance().GetSettingsState().CurrentMode == Settings.PCOExplorerMode.TEAMS)
            {
                ownerlist = ownerlist.OrderByDescending(g => g.Key);
            }

            //ownerlist.Add(new Author("Unselected", "Unselected"));
            return ownerlist;
        }
        protected SfLinearGauge GetBarGraph()
        {
            SfLinearGauge sfLinearGauge = new SfLinearGauge();
            sfLinearGauge.Axis.Minimum = 0;
            sfLinearGauge.Axis.Maximum = 101;
            sfLinearGauge.Axis.ShowLabels = false;
            sfLinearGauge.Axis.ShowTicks = false;
            sfLinearGauge.Axis.AxisLineStrokeThickness = 25;
            sfLinearGauge.Axis.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            sfLinearGauge.Axis.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            sfLinearGauge.Axis.Margin = new Microsoft.UI.Xaml.Thickness(1,2,1,2);
            sfLinearGauge.Axis.AxisLineStroke = new SolidColorBrush(Color.FromArgb(255,0,0,0));

            var blocks = new List<GraphBlock>();
            if (this.GraphModel.LinesTotal > 0)
            {
                if (this.GetType() == typeof(PCOFile))
                {
                    blocks = GraphHelper.GetGraphBlocksFromDistribution(this.GraphModel, ((PCOFile)this).Creator);
                }
                else
                {
                    blocks = GraphHelper.GetGraphBlocksFromDistribution(this.GraphModel);
                }
            } else
            {
                this.GraphModel.SuggestedOwner = null;
            }



            foreach (var block in blocks)
            {
                LinearGaugeRange gaugeRange = new LinearGaugeRange();
                gaugeRange.StartValue = block.StartValue + 0.5;
                gaugeRange.EndValue = block.EndValue + 0.5;
                gaugeRange.StartWidth = 22;
                gaugeRange.EndWidth = 22;
                gaugeRange.RangePosition = GaugeElementPosition.Cross;
                
                gaugeRange.Background = new SolidColorBrush(block.Color);

                if (block.EndValue - block.StartValue > 10)
                {
                    var panel = new StackPanel();
                    panel.Orientation = Orientation.Horizontal;
                    panel.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;

                    if (block.IsCreator)
                    {
                        Image img = new Image();
                        img.Source = new BitmapImage(new Uri("ms-appx:///Assets/star.png"));
                        img.Height = 20;
                        img.Width = 20;
                        img.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                        panel.Children.Add(img);
                    }

                    if (!block.IsActive)
                    {
                        Image img = new Image();
                        img.Source = new BitmapImage(new Uri("ms-appx:///Assets/noBW.png"));
                        img.Height = 20;
                        img.Width = 20;
                        img.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                        panel.Children.Add(img);
                    }

                    gaugeRange.Child = panel;
                }

                var tooltip = new ToolTip();
                var tooltipMsg = new TextBlock();
                tooltipMsg.Text = block.ToolTip;
                tooltipMsg.HorizontalTextAlignment = Microsoft.UI.Xaml.TextAlignment.Center;
                tooltip.Content = tooltipMsg;

                ToolTipService.SetToolTip(gaugeRange, tooltip);

                sfLinearGauge.Axis.Ranges.Add(gaugeRange);
            }
            _bargraph = sfLinearGauge;
            return sfLinearGauge;
        }

        protected SfLinearGauge _bargraph { get; set; }

        public void GenerateBarGraph()
        {
            this.GetBarGraph();
        }

        public string GetRelativePath(bool useForwardSlashes = false)
        {
            PCOFolder tempFolder = this.Parent;
            string relativePath = "";
            string seperator = useForwardSlashes ? "/" : "\\";
            if (this is PCOFolder folder)
            {
                tempFolder = folder;
            } else
            {
                relativePath = this.Name;
            }
            while (tempFolder?.Parent != null)
            {
                relativePath = tempFolder.Name + seperator + relativePath;
                tempFolder = tempFolder.Parent;
            }
            return relativePath;
        }
        
    }
}

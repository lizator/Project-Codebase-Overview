using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.Graphs.Model;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview.UserControles
{
    public sealed partial class PCOBarGraph : UserControl
    {
        List<GraphBlock> Blocks = new List<GraphBlock>();
        
        public ObservableCollection<GraphBlock> ItemsSource
        {
            get { return (ObservableCollection<GraphBlock>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<GraphBlock>), typeof(PCOBarGraph), new PropertyMetadata(null, OnItemsSourceChanged));



        public PCOBarGraph()
        {
            this.InitializeComponent();
        }

        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var barGraph = (PCOBarGraph)obj;
            barGraph.ChangeItems(args.NewValue);
            

        }

        private void ChangeItems(object newVal)
        {
            Blocks = new List<GraphBlock>(((ObservableCollection<GraphBlock>)newVal).ToList());
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.Children.Clear();

            var index = 0;
            foreach (var block in Blocks)
            {
                ColumnDefinition def = new ColumnDefinition();
                def.Width = new GridLength(block.Percentage/100, GridUnitType.Star);

                Grid inner = new Grid();
                inner.Background = new SolidColorBrush(block.Color);
                inner.Height = 100;
                inner.SetValue(Grid.ColumnProperty, index++);
                //Grid.SetColumn(inner, index);
                MainGrid.ColumnDefinitions.Add(def);
                MainGrid.Children.Add(inner);
            }
        }
    }
}

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project_Codebase_Overview.DataCollection.Model;
using Syncfusion.UI.Xaml.Core;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_Codebase_Overview.UserControls
{
    public sealed partial class BarGraphCellControl : UserControl
    {
        public BarGraphCellControl()
        {
            this.InitializeComponent();
        }

        public GraphModel GraphData
        {
            get => (GraphModel)GetValue(GraphDataProperty);
            set => ImportGraphData(GraphDataProperty, value);
        }

        private void ImportGraphData(DependencyProperty GraphDataProperty, GraphModel gm)
        {

            SfLinearGauge sfLinearGauge = new SfLinearGauge();
            sfLinearGauge.Axis.Maximum = 140;
            sfLinearGauge.Axis.Interval = 10;

            LinearGaugeRange gaugeRange1 = new LinearGaugeRange();
            gaugeRange1.StartValue = 0;
            gaugeRange1.EndValue = 50;
            gaugeRange1.Background = new SolidColorBrush(Colors.Red);
            sfLinearGauge.Axis.Ranges.Add(gaugeRange1);

            LinearGaugeRange gaugeRange2 = new LinearGaugeRange();
            gaugeRange2.StartValue = 50;
            gaugeRange2.EndValue = 100;
            gaugeRange2.Background = new SolidColorBrush(Colors.Orange);
            sfLinearGauge.Axis.Ranges.Add(gaugeRange2);

            LinearGaugeRange gaugeRange3 = new LinearGaugeRange();
            gaugeRange3.StartValue = 100;
            gaugeRange3.EndValue = 150;
            gaugeRange3.Background = new SolidColorBrush(Colors.Green);
            sfLinearGauge.Axis.Ranges.Add(gaugeRange3);

            this.Content = sfLinearGauge;

            SetValue(GraphDataProperty, gm);
        }


        DependencyProperty GraphDataProperty = DependencyProperty.Register(
            nameof(GraphData),
            typeof(GraphModel),
            typeof(BarGraphCellControl),
            new PropertyMetadata(default(GraphModel), new PropertyChangedCallback(OnDataChanged)));


        public SfLinearGauge Graph
        {
            get => (SfLinearGauge)GetValue(GraphProperty);
            set => SetValue(GraphProperty, value);
        }

        DependencyProperty GraphProperty = DependencyProperty.Register(
            nameof(Graph),
            typeof(SfLinearGauge),
            typeof(BarGraphCellControl),
            new PropertyMetadata(default(SfLinearGauge), new PropertyChangedCallback(OnGraphChanged)));

        public bool HasGraphModel { get; set; }

        private static void OnGraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            return;
        }
        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BarGraphCellControl BarGraphControl = d as BarGraphCellControl; //null checks omitted
            GraphModel gm = e.NewValue as GraphModel; //null checks omitted
            if (gm.LinesTotal == 0)
            {
                BarGraphControl.HasGraphModel = false;
            }
            else
            {
                BarGraphControl.HasGraphModel = true;
            }

            SfLinearGauge sfLinearGauge = new SfLinearGauge();
            sfLinearGauge.Axis.Minimum = 0;
            sfLinearGauge.Axis.Maximum = 100;

            LinearGaugeRange gaugeRange1 = new LinearGaugeRange();
            gaugeRange1.StartValue = 0;
            gaugeRange1.EndValue = 50;
            gaugeRange1.Background = new SolidColorBrush(Colors.Red);
            sfLinearGauge.Axis.Ranges.Add(gaugeRange1);

            LinearGaugeRange gaugeRange2 = new LinearGaugeRange();
            gaugeRange2.StartValue = 50;
            gaugeRange2.EndValue = 100;
            gaugeRange2.Background = new SolidColorBrush(Colors.Orange);
            sfLinearGauge.Axis.Ranges.Add(gaugeRange2);

            LinearGaugeRange gaugeRange3 = new LinearGaugeRange();
            gaugeRange3.StartValue = 100;
            gaugeRange3.EndValue = 150;
            gaugeRange3.Background = new SolidColorBrush(Colors.Green);
            sfLinearGauge.Axis.Ranges.Add(gaugeRange3);

            BarGraphControl.Graph = sfLinearGauge;
            
        }
    }
}

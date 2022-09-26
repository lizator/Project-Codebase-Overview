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
using Windows.UI;

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
            set => SetValue(GraphDataProperty, value);
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
            double currentStartPos = 0;

            //testing purposes adding colors
            SolidColorBrush[] colors = {
                new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
                new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
                };
            int count = 0;

            foreach (var dist in gm.LineDistribution)
            {

                LinearGaugeRange gaugeRange = new LinearGaugeRange();
                gaugeRange.StartValue = currentStartPos;
                gaugeRange.EndValue = ((double)dist.Value / (double)gm.LinesTotal) * 100 + currentStartPos;
                gaugeRange.StartWidth = 25;
                gaugeRange.EndWidth = 25;
                gaugeRange.Background = colors[count % 3];
                count++;

                currentStartPos = gaugeRange.EndValue;

                sfLinearGauge.Axis.Ranges.Add(gaugeRange);
            }

            BarGraphControl.Graph = sfLinearGauge;
            
        }
    }
}

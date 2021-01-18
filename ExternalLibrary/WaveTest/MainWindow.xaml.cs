using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
//using Sparrow;
//using Sparrow.Chart;
using OxyPlot;
using OxyPlot.Axes;

namespace WaveTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlotModel WaveformModel { get; private set; }
        public PlotModel SpectrumModel { get; private set; }
        
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            WaveformModel = new PlotModel();
            WaveformModel.Title = "test";

            var axisX = new LinearAxis { Position = AxisPosition.Bottom, Title="hoge" };
            WaveformModel.Axes.Add(axisX);

            var axisY = new LinearAxis { Position = AxisPosition.Left, Title="power" };
            WaveformModel.Axes.Add(axisY);

            WaveformChart.Model = WaveformModel;

            SpectrumModel = new PlotModel();
        }

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            float time = float.Parse(timeBox.Text);
            int samplingRate = int.Parse(samplingRateBox.Text);
            float frequency = float.Parse(sinWaveBox.Text);

            float dt = 1f / (float)samplingRate;
            int totalSample = (int)(time / dt);

        }

        private void analysisButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;

namespace WaveTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public SpectrumVM spectrumVM { get; } = new SpectrumVM();

        public WaveformVM waveformVM { get; } = new WaveformVM();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            spectrumVM.Init();
            waveformVM.Init();
        }

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void analysisButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public abstract class ViewModel
    {
        public PlotModel Model { get; } = new PlotModel();
        public PlotController Controller { get; } = new PlotController();

        public OxyPlot.Axes.TimeSpanAxis X { get; } = new OxyPlot.Axes.TimeSpanAxis();

        public OxyPlot.Axes.LinearAxis Y { get; } = new OxyPlot.Axes.LinearAxis();

        public LineSeries Line { get; } = new LineSeries();

        public ViewModel()
        {

        }

        public abstract void Init();
    }

    public class SpectrumVM : ViewModel
    {
        

        public override void Init()
        {
            Model.Title = "Spectrums";

            X.Position = OxyPlot.Axes.AxisPosition.Bottom;
            Y.Position = OxyPlot.Axes.AxisPosition.Left;

            Model.Axes.Add(X);
            Model.Axes.Add(Y);

            Model.InvalidatePlot(true);
        }
    }

    public class WaveformVM : ViewModel
    {
        public override void Init()
        {
            Model.Title = "Waveforms";

            X.Position = OxyPlot.Axes.AxisPosition.All;
            Y.Position = OxyPlot.Axes.AxisPosition.Left;

            Model.Axes.Add(X);
            Model.Axes.Add(Y);

            Model.InvalidatePlot(true);
        }
    }
}

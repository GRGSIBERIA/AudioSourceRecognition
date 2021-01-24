using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace GuitarRecorder
{
    public partial class GuitarRecogApp : Form
    {
        AsioOut device = null;
        WaveFormat format;
        BufferedWaveProvider provider;
        WaveStream stream;

        bool isAnalyzing = false;

        void InitializeFormat()
        {
            int frequency = int.Parse(textBoxSamplingFrequency.Text);
            int bits = int.Parse(textBoxBits.Text);
            format = new WaveFormat(int.Parse(textBoxSamplingFrequency.Text), bits, 1);
        }

        public GuitarRecogApp()
        {
            InitializeComponent();

            // デバイスの一覧を取得
            foreach (var name in AsioOut.GetDriverNames())
            {
                comboBoxInputDevice.Items.Add(name);
            }
        }

        private void 設定CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Settings();
            form.Show();
        }

        private void comboBoxInputDevice_DataSourceChanged(object sender, EventArgs e)
        {
            // 選択されたデバイスを探索して、使用デバイスとして登録する
            foreach (var name in AsioOut.GetDriverNames())
            {
                if (comboBoxInputDevice.Text == name)
                    device = new AsioOut(name);
            }

            if (device != null)
            {
                comboBoxInputChannel.Items.Clear();
                for (int i = 0; i < device.NumberOfInputChannels; ++i)
                    comboBoxInputChannel.Items.Add(device.AsioInputChannelName(i));
                device.InputChannelOffset = 0;  // デフォルトでは0番
            }
        }

        private void wavefromSetting_DataSourceChanged(object sender, EventArgs e)
        {
            InitializeFormat();
        }

        private void buttonAnalyze_Click(object sender, EventArgs e)
        {
            if (!isAnalyzing)
            {
                int samplingRate = int.Parse(textBoxSamplingFrequency.Text);

                isAnalyzing = true;
                buttonAnalyze.Text = "停止";

                waveViewer.WaveStream = new WaveMixerStream32();

                // プロバイダを作成してデバイスを再生する
                int samplingrate = int.Parse(textBoxSamplingFrequency.Text);
                provider = new BufferedWaveProvider(new WaveFormat(samplingrate, 32, 1));
                device.Init(provider);  // デバイスを初期化
                device.AudioAvailable += CanBuffering;
                device.Play();
            }
            else
            {
                isAnalyzing = false;
                buttonAnalyze.Text = "解析";

                device.Stop();
                device.AudioAvailable -= CanBuffering;  // ハンドラから削除する
            }
        }

        private void comboBoxInputChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            device.InputChannelOffset = comboBoxInputChannel.SelectedIndex;
        }

        int testNum = 0;

        private void CanBuffering(object sender, EventArgs e)
        {
            Console.WriteLine($"{++testNum}");
        }
    }
}

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
using NAudio.Mixer;

namespace GuitarRecorder
{
    public partial class GuitarRecogApp : Form
    {
        WaveFormat format;

        WaveIn input;
        WaveInCapabilities capability;
        MixerLine mixer;
        BufferedWaveProvider provider;

        bool isAnalyzing = false;

        void InitializeFormat()
        {
            int frequency = int.Parse(textBoxSamplingFrequency.Text);
            int bits = int.Parse(textBoxBits.Text);
            format = new WaveFormat(frequency, bits, 1);
        }

        public GuitarRecogApp()
        {
            InitializeComponent();

            input = new WaveIn();
            mixer = input.GetMixerLine();
            input.DataAvailable += new EventHandler<WaveInEventArgs>(CanBuffering);

            for (int i = 0; i < WaveIn.DeviceCount; ++i)
                comboBoxInputDevice.Items.Add(WaveIn.GetCapabilities(i).ProductName);
        }

        private void 設定CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Settings();
            form.Show();
        }

        private void comboBoxInputDevice_DataSourceChanged(object sender, EventArgs e)
        {
            comboBoxInputChannel.Items.Clear();
            capability = WaveIn.GetCapabilities(comboBoxInputDevice.SelectedIndex);

            for (int i = 0; i < capability.Channels; ++i)
                comboBoxInputChannel.Items.Add(i.ToString());
        }

        private void wavefromSetting_DataSourceChanged(object sender, EventArgs e)
        {
            InitializeFormat();
        }

        private void buttonAnalyze_Click(object sender, EventArgs e)
        {
            if (!isAnalyzing)
            {
                /**
                if (device != null)
                {
                    device.Dispose();
                    device = new AsioOut(comboBoxInputDevice.Text);
                }

                

                stream = new WaveMixerStream32();
                waveViewer.WaveStream = stream;
                waveViewer.BackColor = Color.White;

                // プロバイダを作成してデバイスを再生する
                InitializeFormat();
                provider = new BufferedWaveProvider(format);
                device.Init(provider);  // デバイスを初期化
                device.AudioAvailable += new EventHandler<AsioAudioAvailableEventArgs>(CanBuffering);
                device.Play();
                */

                isAnalyzing = true;
                buttonAnalyze.Text = "停止";

                InitializeFormat();

                provider = new BufferedWaveProvider(format);
                provider.DiscardOnBufferOverflow = true;

                input.DeviceNumber = comboBoxInputDevice.SelectedIndex;
                input.WaveFormat = format;
                input.StartRecording();
            }
            else
            {
                isAnalyzing = false;
                buttonAnalyze.Text = "解析";
                input.StopRecording();
            }
        }

        private void comboBoxInputChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            //device.InputChannelOffset = comboBoxInputChannel.SelectedIndex;
        }

        int count = 0;

        private void CanBuffering(object sender, WaveInEventArgs e)
        {
            provider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
    }
}

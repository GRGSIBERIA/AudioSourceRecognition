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
        MMDevice device;
        WaveFormat format;

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
            var enumerator = new MMDeviceEnumerator();
            for (int i = 0; i < WaveIn.DeviceCount; ++i)
            {
                var item = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                comboBoxInputDevice.Items.Add(item[i].FriendlyName);
            }
            enumerator.Dispose();

            InitializeFormat();
        }

        private void 設定CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Settings();
            form.Show();
        }

        private void comboBoxInputDevice_DataSourceChanged(object sender, EventArgs e)
        {
            // 選択されたデバイスを探索して、使用デバイスとして登録する
            var enumerator = new MMDeviceEnumerator();
            for (int i = 0; i < WaveIn.DeviceCount; ++i)
            {
                var item = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                if (comboBoxInputDevice.Text == item[i].FriendlyName)
                {
                    device = item[i];
                    break;
                }
            }
        }

        private void wavefromSetting_DataSourceChanged(object sender, EventArgs e)
        {
            InitializeFormat();
        }
    }
}

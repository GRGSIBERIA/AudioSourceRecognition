using System.Windows.Forms;
using System;
using System.Collections;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.IO;
using System.Text.Json;

namespace GuitarRecorder
{
    public partial class Settings : Form
    {
        const string path = "./appsettings.json";

        [Serializable]
        struct SettingData
        {
            public string friendlyName;
            public int samplingFrequency;
            public int sampleN;
            public int shiftTime;
        }

        /// <summary>
        /// 各種項目を埋める
        /// </summary>
        /// <param name="data">設定データ</param>
        void SetSettings(SettingData data)
        {
            this.comboBoxAudioIF.Text = data.friendlyName;
            this.textBoxSamplingFrequency.Text = data.samplingFrequency.ToString();
            this.textBoxMadoSample.Text = data.sampleN.ToString();
            this.textBoxShiftTimes.Text = data.shiftTime.ToString();

            this.labelRealSampleN.Text = $"= {1 << data.sampleN} 点";
            this.labelRealShiftTime.Text = $"= {1 << data.shiftTime} 回";
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="file">保存するFileStream</param>
        void SaveSettings(StreamWriter file)
        {
            var data = new SettingData();
            data.friendlyName = this.comboBoxAudioIF.SelectedItem as string;
            data.samplingFrequency = int.Parse(this.textBoxSamplingFrequency.Text);
            data.sampleN = int.Parse(this.textBoxMadoSample.Text);
            data.shiftTime = int.Parse(this.textBoxShiftTimes.Text);

            string json = JsonSerializer.Serialize(data);
            file.Write(json);
        }

        /// <summary>
        /// 設定ファイルを持っているか確認する
        /// </summary>
        void hasSettingFile()
        {
            
            if (!File.Exists(path))
            {
                using (var file = new StreamWriter(path))
                {
                    // 何もしない
                    var settings = new SettingData();
                    settings.friendlyName = "";
                    settings.samplingFrequency = 44100;
                    settings.sampleN = 10;
                    settings.shiftTime = 2;

                    // 初期データを入力しておく
                    SetSettings(settings);
                    SaveSettings(file);
                }
            }
            else
            {
                using (var file = new StreamReader(path))
                {
                    string data = file.ReadToEnd();
                    var settings = JsonSerializer.Deserialize<SettingData>(data);

                    SetSettings(settings);
                }
            }
        }

        public Settings()
        {
            InitializeComponent();

            // 使用可能なデバイスを列挙する
            var enumerator = new MMDeviceEnumerator();
            for (int i = 0; i < WaveOut.DeviceCount; ++i)
            {
                var collection = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)[i];
                comboBoxAudioIF.Items.Add(collection.FriendlyName);
            }
            enumerator.Dispose();   // Disposeを呼び出さなければならない

            hasSettingFile();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            using (var file = new StreamWriter(path))
            {
                SaveSettings(file);
            }
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (var file = new StreamWriter(path))
            {
                SaveSettings(file);
            }
        }
    }
}

using System.Windows.Forms;
using System;
using System.Collections;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GuitarRecorder
{
    public partial class Settings : Form
    {
        void hasSettingFile()
        {
            if (File.Exists("./appsettings.json"))
            {

            }
            else
            {
                var builder = new ConfigurationBuilder();
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.json");
                builder.Build();
            }
        }

        public Settings()
        {
            InitializeComponent();

            //comboBoxAudioIF.SelectedIndex = 0;
            var enumerator = new MMDeviceEnumerator();
            for (int i = 0; i < WaveOut.DeviceCount; ++i)
            {
                var collection = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)[i];
                comboBoxAudioIF.Items.Add(collection.FriendlyName);
            }
            enumerator.Dispose();
        }
    }
}

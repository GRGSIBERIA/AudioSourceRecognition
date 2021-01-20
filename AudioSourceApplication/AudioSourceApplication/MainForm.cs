using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AudioSourceApplication
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        WavFile wav;

        /// <summary>
        /// WAVファイルの読み込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadWAVLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "WAV|*.wav";
            ofd.Title = @"解析対象のWAVファイルの読み込み";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (BinaryReader br = new BinaryReader(ofd.OpenFile()))
                {
                    wav = new WavFile(br);
                }
            }
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

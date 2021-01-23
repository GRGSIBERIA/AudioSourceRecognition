
namespace GuitarRecorder
{
    partial class GuitarRecogApp
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ファイルFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.設定CToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBoxInputDevice = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxInputChannel = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSamplingFrequency = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxBits = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルFToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 26);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ファイルFToolStripMenuItem
            // 
            this.ファイルFToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.設定CToolStripMenuItem});
            this.ファイルFToolStripMenuItem.Name = "ファイルFToolStripMenuItem";
            this.ファイルFToolStripMenuItem.Size = new System.Drawing.Size(89, 22);
            this.ファイルFToolStripMenuItem.Text = "ファイル (&F)";
            // 
            // 設定CToolStripMenuItem
            // 
            this.設定CToolStripMenuItem.Name = "設定CToolStripMenuItem";
            this.設定CToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.設定CToolStripMenuItem.Text = "設定 (&C)";
            this.設定CToolStripMenuItem.Click += new System.EventHandler(this.設定CToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 132);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "解析";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // comboBoxInputDevice
            // 
            this.comboBoxInputDevice.FormattingEnabled = true;
            this.comboBoxInputDevice.Location = new System.Drawing.Point(83, 31);
            this.comboBoxInputDevice.Name = "comboBoxInputDevice";
            this.comboBoxInputDevice.Size = new System.Drawing.Size(282, 20);
            this.comboBoxInputDevice.TabIndex = 2;
            this.comboBoxInputDevice.SelectedIndexChanged += new System.EventHandler(this.comboBoxInputDevice_DataSourceChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(381, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "入力チャンネル";
            // 
            // comboBoxInputChannel
            // 
            this.comboBoxInputChannel.FormattingEnabled = true;
            this.comboBoxInputChannel.Location = new System.Drawing.Point(462, 32);
            this.comboBoxInputChannel.Name = "comboBoxInputChannel";
            this.comboBoxInputChannel.Size = new System.Drawing.Size(49, 20);
            this.comboBoxInputChannel.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "デバイス一覧";
            // 
            // textBoxSamplingFrequency
            // 
            this.textBoxSamplingFrequency.Location = new System.Drawing.Point(135, 58);
            this.textBoxSamplingFrequency.Name = "textBoxSamplingFrequency";
            this.textBoxSamplingFrequency.Size = new System.Drawing.Size(57, 19);
            this.textBoxSamplingFrequency.TabIndex = 6;
            this.textBoxSamplingFrequency.Text = "48000";
            this.textBoxSamplingFrequency.TextChanged += new System.EventHandler(this.wavefromSetting_DataSourceChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "サンプリング周波数 [Hz]";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(198, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "量子化ビット数";
            // 
            // textBoxBits
            // 
            this.textBoxBits.Location = new System.Drawing.Point(280, 58);
            this.textBoxBits.Name = "textBoxBits";
            this.textBoxBits.Size = new System.Drawing.Size(41, 19);
            this.textBoxBits.TabIndex = 9;
            this.textBoxBits.Text = "16";
            this.textBoxBits.TextChanged += new System.EventHandler(this.wavefromSetting_DataSourceChanged);
            // 
            // GuitarRecogApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBoxBits);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxSamplingFrequency);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxInputChannel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxInputDevice);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GuitarRecogApp";
            this.Text = "Guitar Recognition";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ファイルFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 設定CToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBoxInputDevice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxInputChannel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSamplingFrequency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxBits;
    }
}


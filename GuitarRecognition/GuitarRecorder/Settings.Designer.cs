
namespace GuitarRecorder
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxAudioIF = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSamplingFrequency = new System.Windows.Forms.TextBox();
            this.textBoxMadoSample = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxShiftTimes = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "オーディオ・インターフェース";
            // 
            // comboBoxAudioIF
            // 
            this.comboBoxAudioIF.FormattingEnabled = true;
            this.comboBoxAudioIF.Location = new System.Drawing.Point(144, 6);
            this.comboBoxAudioIF.Name = "comboBoxAudioIF";
            this.comboBoxAudioIF.Size = new System.Drawing.Size(235, 20);
            this.comboBoxAudioIF.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "サンプリング周波数 [Hz]";
            // 
            // textBoxSamplingFrequency
            // 
            this.textBoxSamplingFrequency.Location = new System.Drawing.Point(144, 33);
            this.textBoxSamplingFrequency.Name = "textBoxSamplingFrequency";
            this.textBoxSamplingFrequency.Size = new System.Drawing.Size(100, 19);
            this.textBoxSamplingFrequency.TabIndex = 3;
            // 
            // textBoxMadoSample
            // 
            this.textBoxMadoSample.Location = new System.Drawing.Point(144, 59);
            this.textBoxMadoSample.Name = "textBoxMadoSample";
            this.textBoxMadoSample.Size = new System.Drawing.Size(100, 19);
            this.textBoxMadoSample.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "窓幅 [2^N]";
            // 
            // textBoxShiftTimes
            // 
            this.textBoxShiftTimes.Location = new System.Drawing.Point(144, 85);
            this.textBoxShiftTimes.Name = "textBoxShiftTimes";
            this.textBoxShiftTimes.Size = new System.Drawing.Size(100, 19);
            this.textBoxShiftTimes.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "窓シフト [回数]";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxShiftTimes);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxMadoSample);
            this.Controls.Add(this.textBoxSamplingFrequency);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxAudioIF);
            this.Controls.Add(this.label1);
            this.Name = "Settings";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxAudioIF;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSamplingFrequency;
        private System.Windows.Forms.TextBox textBoxMadoSample;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxShiftTimes;
        private System.Windows.Forms.Label label4;
    }
}
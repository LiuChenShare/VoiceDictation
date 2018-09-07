using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NAudioDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
            Directory.CreateDirectory(outputFolder);
            var outputFilePath = Path.Combine(outputFolder, "recorded.wav");

            var waveIn = new WaveInEvent();

            WaveFileWriter writer = null;
            bool closing = false;
            var f = new Form();
            var buttonRecord = new Button() { Text = "Record" };
            var buttonStop = new Button() { Text = "Stop", Left = buttonRecord.Right, Enabled = false };
            f.Controls.AddRange(new Control[] { buttonRecord, buttonStop });

            buttonRecord.Click += (s, a) =>
            {
                writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
                waveIn.StartRecording();
                buttonRecord.Enabled = false;
                buttonStop.Enabled = true;
            };

            buttonStop.Click += (s, a) => waveIn.StopRecording();

            waveIn.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 30)
                {
                    waveIn.StopRecording();
                }
            };

            waveIn.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
                writer = null;
                buttonRecord.Enabled = true;
                buttonStop.Enabled = false;
                if (closing)
                {
                    waveIn.Dispose();
                }
            };

            f.FormClosing += (s, a) => { closing = true; waveIn.StopRecording(); };
            f.ShowDialog();
        }
    }
}

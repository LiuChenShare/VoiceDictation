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
using System.Windows.Shapes;

namespace Project_001_初试
{
    /// <summary>
    /// NAudioDemo.xaml 的交互逻辑
    /// </summary>
    public partial class NAudioDemo : Window
    {
        public NAudioDemo()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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

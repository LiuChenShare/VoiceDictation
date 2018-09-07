using NAudio.Wave;
using Project_001_初试.XunFei;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_001_初试
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        WaveFileWriter m_waveFile;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window2 isw = new Window2();
            isw.ShowDialog();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Window1 isw = new Window1();
            isw.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //初始化
            String FilePath = AppDomain.CurrentDomain.BaseDirectory + "Temp.wav";
            WaveIn m_waveSource = new WaveIn();
            m_waveSource.WaveFormat = new NAudio.Wave.WaveFormat(16000, 16, 1);// 16bit,16KHz,Mono的录音格式
            m_waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            m_waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);
            m_waveFile = new WaveFileWriter(FilePath, m_waveSource.WaveFormat);

            //开始录音
            m_waveSource.StartRecording();

            //停止录音
            m_waveSource.StopRecording();
        }

        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            throw new NotImplementedException();
        }

        //保存到截获到的声音
        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (m_waveFile != null)
            {
                m_waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                m_waveFile.Flush();
            }
        }

        public string AudioToString(string inFile)
        {
            MSCHelper.MSPLogin(null, null, "appid=5b91df7f");
            int ret = 0;
            string text = String.Empty;
            FileStream fileStream = new FileStream(inFile, FileMode.OpenOrCreate);
            byte[] array = new byte[this.BUFFER_NUM];
            IntPtr intPtr = Marshal.AllocHGlobal(this.BUFFER_NUM);
            int audioStatus = 2;
            int epStatus = -1;
            int recogStatus = -1;
            int rsltStatus = -1;
            while (fileStream.Position != fileStream.Length)
            {
                int waveLen = fileStream.Read(array, 0, this.BUFFER_NUM);
                Marshal.Copy(array, 0, intPtr, array.Length);
                ret = MSCHelper.QISRAudioWrite(this.m_sessionID, intPtr, (uint)waveLen, audioStatus, ref epStatus, ref recogStatus);
                if (ret != 0)
                {
                    fileStream.Close();
                    throw new Exception("QISRAudioWrite err,errCode=" + ret);
                }
                if (recogStatus == 0)
                {
                    IntPtr intPtr2 = MSCHelper.QISRGetResult(this.m_sessionID, ref rsltStatus, 0, ref ret);
                    if (intPtr2 != IntPtr.Zero)
                    {
                        text += this.Ptr2Str(intPtr2);
                    }
                }
                Thread.Sleep(500);
            }
            fileStream.Close();
            audioStatus = 4;
            ret = MSCHelper.QISRAudioWrite(this.m_sessionID, intPtr, 1u, audioStatus, ref epStatus, ref recogStatus);
            if (ret != 0)
            {
                throw new Exception("QISRAudioWrite write last audio err,errCode=" + ret);
            }
            int timesCount = 0;
            while (true)
            {
                IntPtr intPtr2 = MSCHelper.QISRGetResult(this.m_sessionID, ref rsltStatus, 0, ref ret);
                if (intPtr2 != IntPtr.Zero)
                {
                    text += this.Ptr2Str(intPtr2);
                }
                if (ret != 0)
                {
                    break;
                }
                Thread.Sleep(200);
                if (rsltStatus == 5 || timesCount++ >= 50)
                {
                    break;
                }
            }
            return text;
        }
    }
}

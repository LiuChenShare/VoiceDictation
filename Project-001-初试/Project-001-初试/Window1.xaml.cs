using NAudio.Wave;
using Project_001_初试.XunFei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using VoiceRecorder.Audio;

namespace Project_001_初试
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        string session_begin_params;
        private WaveIn waveIn;
        private AudioRecorder recorder;
        private float lastPeak;//说话音量
        float secondsRecorded;
        float totalBufferLength;
        int Ends = 5;
        private const int BUFFER_SIZE = 4096;
        List<VoiceData> VoiceBuffer = new List<VoiceData>();

        public Window1()
        {
            InitializeComponent();
        }

        public void SpeechRecognition()//初始化语音识别
        {
            int ret = (int)ErrorCode.MSP_SUCCESS;
            string login_params = string.Format("appid=5b91df7f,word_dir= . ");//appid和msc.dll要配套
            #region 参数
            /*
            *sub:本次识别请求的类型  iat 连续语音识别;   asr 语法、关键词识别,默认为iat
            *domain:领域      iat：连续语音识别  asr：语法、关键词识别    search：热词   video：视频    poi：地名  music：音乐    默认为iat。 注意：sub=asr时，domain只能为asr
            *language:语言    zh_cn：简体中文  zh_tw：繁体中文  en_us：英文    默认值：zh_cn
            *accent:语言区域    mandarin：普通话    cantonese：粤语    lmz：四川话 默认值：mandarin
            *sample_rate:音频采样率  可取值：16000，8000  默认值：16000   离线识别不支持8000采样率音频
            *result_type:结果格式   可取值：plain，json  默认值：plain
            *result_encoding:识别结果字符串所用编码格式  GB2312;UTF-8;UNICODE    不同的格式支持不同的编码：   plain:UTF-8,GB2312  json:UTF-8
            */
            #endregion
            session_begin_params = "sub=iat,domain=iat,language=zh_cn,accent=mandarin,sample_rate=16000,result_type=plain,result_encoding=utf8";

            string Username = "";
            string Password = "";
            ret = MSCDLL.MSPLogin(Username, Password, login_params);

            if ((int)ErrorCode.MSP_SUCCESS != ret)//不成功
            {
                //Console.WriteLine("失败了");
                Console.WriteLine("MSPLogin failed,error code:{0}", ret.ToString());
                MSCDLL.MSPLogout();
            }
        }
        private WaveIn CreateWaveInDevice()//WaveIn实例化
        {
            WaveIn newWaveIn = new WaveIn();
            //newWaveIn.WaveFormat = new WaveFormat(16000, 16, 1); // 16bit,16KHz,Mono的录音格式
            newWaveIn.WaveFormat = new WaveFormat(16000, 1);//16bit,1KHz 的录音格式
            newWaveIn.DataAvailable += OnDataAvailable;
            newWaveIn.RecordingStopped += OnRecordingStopped;
            return newWaveIn;
        }
        /// <summary>
        /// 开始录音回调函数       保存截获到的声音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            totalBufferLength += e.Buffer.Length;
            secondsRecorded = (float)(totalBufferLength / 32000);

            VoiceData data = new VoiceData();
            for (int i = 0; i < 3200; i++)
            {
                data.data[i] = e.Buffer[i];
            }
            VoiceBuffer.Add(data);

            if (lastPeak < 20)
                Ends = Ends - 1;
            else
                Ends = 5;

            if (Ends == 0)
            {
                if (VoiceBuffer.Count() > 5)
                {
                    RunIAT(VoiceBuffer, session_begin_params);//调用语音识别
                }

                VoiceBuffer.Clear();
                Ends = 5;
            }

        }
        /// <summary>
        /// 录音结束回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                /* MessageBox.Show(String.Format("A problem was encountered during recording {0}",
                                               e.Exception.Message));*/
                Console.WriteLine("A problem was encountered during recording {0}", e.Exception.Message);
            }
        }
        /// <summary>
        /// 指针转字符串
        /// </summary>
        /// <param name="p">指向非托管代码字符串的指针</param>
        /// <returns>返回指针指向的字符串</returns>
        private string PtrToStr(IntPtr p)
        {
            List<byte> lb = new List<byte>();
            try
            {
                while (Marshal.ReadByte(p) != 0)
                {
                    lb.Add(Marshal.ReadByte(p));
                    p = p + 1;
                }
            }
            catch (AccessViolationException ex)
            {
                SetText(String.Format(ex.Message));
            }
            return Encoding.UTF8.GetString(lb.ToArray());
        }
        //语音识别
        private void RunIAT(List<VoiceData> VoiceBuffer, string session_begin_params)
        {
            IntPtr session_id = IntPtr.Zero;
            string rec_result = string.Empty;
            string hints = "正常结束";
            AudioStatus aud_stat = AudioStatus.ISR_AUDIO_SAMPLE_CONTINUE;
            EpStatus ep_stat = EpStatus.ISR_EP_LOOKING_FOR_SPEECH;
            RecogStatus rec_stat = RecogStatus.ISR_REC_STATUS_SUCCESS;
            int errcode = (int)ErrorCode.MSP_SUCCESS;

            session_id = MSCDLL.QISRSessionBegin(null, session_begin_params, ref errcode);
            if ((int)ErrorCode.MSP_SUCCESS != errcode)
            {
                SetText("\nQISRSessionBegin failed! error code:" + errcode);
                return;
            }

            for (int i = 0; i < VoiceBuffer.Count(); i++)
            {
                aud_stat = AudioStatus.ISR_AUDIO_SAMPLE_CONTINUE;
                if (i == 0)
                    aud_stat = AudioStatus.ISR_AUDIO_SAMPLE_FIRST;
                errcode = MSCDLL.QISRAudioWrite(PtrToStr(session_id), VoiceBuffer[i].data, (uint)VoiceBuffer[i].data.Length, aud_stat, ref ep_stat, ref rec_stat);
                if ((int)ErrorCode.MSP_SUCCESS != errcode)
                {
                    MSCDLL.QISRSessionEnd(PtrToStr(session_id), null);
                }
            }

            errcode = MSCDLL.QISRAudioWrite(PtrToStr(session_id), null, 0, AudioStatus.ISR_AUDIO_SAMPLE_LAST, ref ep_stat, ref rec_stat);
            if ((int)ErrorCode.MSP_SUCCESS != errcode)
            {
                SetText("\nQISRAudioWrite failed! error code:" + errcode);
                return;
            }

            while (RecogStatus.ISR_REC_STATUS_SPEECH_COMPLETE != rec_stat)
            {
                IntPtr rslt = MSCDLL.QISRGetResult(PtrToStr(session_id), ref rec_stat, 0, ref errcode);
                if ((int)ErrorCode.MSP_SUCCESS != errcode)
                {
                    SetText("\nQISRGetResult failed, error code: " + errcode);
                    break;
                }
                if (IntPtr.Zero != rslt)
                {
                    string tempRes = PtrToStr(rslt);

                    rec_result = rec_result + tempRes;
                    if (rec_result.Length >= BUFFER_SIZE)
                    {
                        SetText("\nno enough buffer for rec_result !\n");
                        break;
                    }
                }

            }
            int errorcode = MSCDLL.QISRSessionEnd(PtrToStr(session_id), hints);

            //语音识别结果
            if (rec_result.Length != 0)
            {

                SetText(rec_result);


                //返回错误代码10111时，可调用SpeechRecognition()函数执行MSPLogin
            }
        }
        //写行
        delegate void SetTextCallback(string text);
        public void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.

            //if (this.textBox1.InvokeRequired)
            /*if (textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.AppendText(text + "\n");
                this.textBox1.Refresh();
            }*/
            this.textBox1.AppendText(text + "\n");
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SpeechRecognition();
            totalBufferLength = 0;
            recorder = new AudioRecorder();
            recorder.BeginMonitoring(-1);
            recorder.SampleAggregator.MaximumCalculated += OnRecorderMaximumCalculated;
            if (waveIn == null)
            {
                waveIn = CreateWaveInDevice();
            }
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.StartRecording();
        }
        void OnRecorderMaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample)) * 100;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();//停止录音
        }
    }
}
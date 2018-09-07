using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_001_初试.XunFei
{
    public class MSCHelper
    {
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="usr"></param>
        /// <param name="pwd"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int MSPLogin(string usr, string pwd, string @params);

        /// <summary>
        /// 开始一次语音听写
        /// </summary>
        /// <param name="grammarList"></param>
        /// <param name="_params"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QISRSessionBegin(string grammarList, string _params, ref int errorCode);

        /// <summary>
        /// 分块写入音频数据
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="grammar"></param>
        /// <param name="type"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRGrammarActivate(string sessionID, string grammar, string type, int weight);

        /// <summary>
        /// 接口返回听写结果
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="waveData"></param>
        /// <param name="waveLen"></param>
        /// <param name="audioStatus"></param>
        /// <param name="epStatus"></param>
        /// <param name="recogStatus"></param>
        /// <returns></returns>
        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRAudioWrite(string sessionID, IntPtr waveData, uint waveLen, int audioStatus, ref int epStatus, ref int recogStatus);

        /// <summary>
        /// 主动结束本次听写
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="rsltStatus"></param>
        /// <param name="waitTime"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr QISRGetResult(string sessionID, ref int rsltStatus, int waitTime, ref int errorCode);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRSessionEnd(string sessionID, string hints);

        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int QISRGetParam(string sessionID, string paramName, string paramValue, ref uint valueLen);

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int MSPLogout();
    }
}
